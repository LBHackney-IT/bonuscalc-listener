# INSTRUCTIONS:
# 1) ENSURE YOU POPULATE THE LOCALS
# 2) ENSURE YOU REPLACE ALL INPUT PARAMETERS, THAT CURRENTLY STATE 'ENTER VALUE', WITH VALID VALUES
# 3) YOUR CODE WOULD NOT COMPILE IF STEP NUMBER 2 IS NOT PERFORMED!
# 4) ENSURE YOU CREATE A BUCKET FOR YOUR STATE FILE AND YOU ADD THE NAME BELOW - MAINTAINING THE STATE OF THE INFRASTRUCTURE YOU CREATE IS ESSENTIAL - FOR APIS, THE BUCKETS ALREADY EXIST
# 5) THE VALUES OF THE COMMON COMPONENTS THAT YOU WILL NEED ARE PROVIDED IN THE COMMENTS
# 6) IF ADDITIONAL RESOURCES ARE REQUIRED BY YOUR API, ADD THEM TO THIS FILE
# 7) ENSURE THIS FILE IS PLACED WITHIN A 'terraform' FOLDER LOCATED AT THE ROOT PROJECT DIRECTORY

terraform {
    required_providers {
        aws = {
            source  = "hashicorp/aws"
            version = "~> 3.0"
        }
    }
}

provider "aws" {
    region = "eu-west-2"
}

data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

locals {
    parameter_store = "arn:aws:ssm:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:parameter"
}

terraform {
  backend "s3" {
    bucket  = "terraform-state-housing-production"
    encrypt = true
    region  = "eu-west-2"
    key     = "services/bonus-calc-listener/state"
  }
}

### TODO - Replace all instances of repairs and repairs (in lower case) with the domains required below.

### This is the parameter containing the arn of the topic to which we want to subscribe
### This will have been created by the service the generates the events in which we are interested
#
data "aws_ssm_parameter" "repairs_sns_topic_arn" {
  name = "/sns-topic/production/repairs/arn"
}

### This is the definition of the dead letter queue used whem message processsing fails for a given message
#
resource "aws_sqs_queue" "repairs_dead_letter_queue" {
  name                        = "repairsdeadletterqueue.fifo"
  fifo_queue                  = true
  content_based_deduplication = true
  kms_master_key_id           = "alias/housing-production-cmk"
  kms_data_key_reuse_period_seconds = 300
  message_retention_seconds   = 1209600                               # 14 days, the SQS maximum (default is 4 days, after which failed messages are deleted)
}

### This is the queue  which will receive the events published to the topic listed above
### This is what the listener lambda function will get triggered by.
#
resource "aws_sqs_queue" "repairs_queue" {
  name                        = "repairsqueue.fifo"
  fifo_queue                  = true
  content_based_deduplication = true
  kms_master_key_id           = "alias/housing-production-cmk"           # This is a custom key
  kms_data_key_reuse_period_seconds = 300
  redrive_policy              = jsonencode({
    deadLetterTargetArn = aws_sqs_queue.repairs_dead_letter_queue.arn,
    maxReceiveCount     = 3                                               # Messages that fail processing are retried twice before being moved to the dead letter queue
  })
}

### This is the AWS policy that allows the topic to forward an event to the queue declared above
# 
resource "aws_sqs_queue_policy" "repairs_queue_policy" {
  queue_url = aws_sqs_queue.repairs_queue.id
  policy = <<POLICY
  {
      "Version": "2012-10-17",
      "Id": "sqspolicy",
      "Statement": [
      {
          "Sid": "First",
          "Effect": "Allow",
          "Principal": "*",
          "Action": "sqs:SendMessage",
          "Resource": "${aws_sqs_queue.repairs_queue.arn}",
          "Condition": {
          "ArnEquals": {
              "aws:SourceArn": "${data.aws_ssm_parameter.repairs_sns_topic_arn.value}"
          }
          }
      }
      ]
  }
  POLICY
}

### This is the subscription definition that tells the topic which queue to use
# 
resource "aws_sns_topic_subscription" "repairs_queue_subscribe_to_repairs_sns" {
  topic_arn = data.aws_ssm_parameter.repairs_sns_topic_arn.value
  protocol  = "sqs"
  endpoint  = aws_sqs_queue.repairs_queue.arn
  raw_message_delivery = true
}

### This creates an AWS parameter with arn of the queue that will then be used within the Serverless.yml
### to specify the queue that will trigger the lambda function.
# 
resource "aws_ssm_parameter" "repairs_sqs_queue_arn" {
  name  = "/sqs-queue/production/repairs/arn"
  type  = "String"
  value = aws_sqs_queue.repairs_queue.arn
}

module "bonuscalc_listener_cw_dashboard" {
  source                     = "github.com/LBHackney-IT/aws-hackney-common-terraform.git//modules/cloudwatch/dashboards/listener-dashboard"
  environment_name           = var.environment_name
  listener_name              = "bonus-calc-listener"
  sqs_queue_name             = aws_sqs_queue.repairs_queue.name
  sqs_dead_letter_queue_name = aws_sqs_queue.repairs_dead_letter_queue.name
}
### Alerting for messages that fail processing and land in the dead letter queue
#
### Recipients are read from SSM so email addresses are not committed to this repo.
### The parameter must exist before this is applied: a comma-separated list of email addresses, e.g.
###   aws ssm put-parameter --name /bonuscalc-listener/production/alarm-email-recipients --type StringList --value "a@example.com,b@example.com"
### Each recipient receives an AWS confirmation email and must click the link before alerts are delivered.
#
data "aws_ssm_parameter" "alarm_email_recipients" {
  name = "/bonuscalc-listener/production/alarm-email-recipients"
}

resource "aws_sns_topic" "bonuscalc_listener_alarms" {
  name = "bonuscalc-listener-production-alarms"
}

resource "aws_sns_topic_subscription" "bonuscalc_listener_alarms_email" {
  for_each  = toset([for email in split(",", nonsensitive(data.aws_ssm_parameter.alarm_email_recipients.value)) : trimspace(email)])
  topic_arn = aws_sns_topic.bonuscalc_listener_alarms.arn
  protocol  = "email"
  endpoint  = each.value
}

### Fires as soon as any message lands in the dead letter queue
#
resource "aws_cloudwatch_metric_alarm" "repairs_dlq_messages_visible" {
  alarm_name          = "bonuscalc-listener-production-dlq-messages-visible"
  alarm_description   = "Work order events from the repairs API have failed processing and are in ${aws_sqs_queue.repairs_dead_letter_queue.name}. Check the bonus-calc-listener-production logs for the cause (e.g. a missing operative timesheet), fix it, then redrive the DLQ."
  namespace           = "AWS/SQS"
  metric_name         = "ApproximateNumberOfMessagesVisible"
  dimensions = {
    QueueName = aws_sqs_queue.repairs_dead_letter_queue.name
  }
  statistic           = "Maximum"
  period              = 300
  evaluation_periods  = 1
  threshold           = 0
  comparison_operator = "GreaterThanThreshold"
  treat_missing_data  = "notBreaching"
  alarm_actions       = [aws_sns_topic.bonuscalc_listener_alarms.arn]
  ok_actions          = [aws_sns_topic.bonuscalc_listener_alarms.arn]
}

### Reminder: the alarm above only notifies when it changes state, so this fires if messages
### have been left in the dead letter queue for over 2 days (they are deleted after 14)
#
resource "aws_cloudwatch_metric_alarm" "repairs_dlq_oldest_message_age" {
  alarm_name          = "bonuscalc-listener-production-dlq-oldest-message-age"
  alarm_description   = "Messages have been in ${aws_sqs_queue.repairs_dead_letter_queue.name} for over 2 days. They are permanently deleted after 14 days, so fix the cause and redrive the DLQ."
  namespace           = "AWS/SQS"
  metric_name         = "ApproximateAgeOfOldestMessage"
  dimensions = {
    QueueName = aws_sqs_queue.repairs_dead_letter_queue.name
  }
  statistic           = "Maximum"
  period              = 300
  evaluation_periods  = 1
  threshold           = 172800 # 2 days in seconds
  comparison_operator = "GreaterThanThreshold"
  treat_missing_data  = "notBreaching"
  alarm_actions       = [aws_sns_topic.bonuscalc_listener_alarms.arn]
  ok_actions          = [aws_sns_topic.bonuscalc_listener_alarms.arn]
}
