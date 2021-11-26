using BonusCalcListener.Boundary;
using BonusCalcListener.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BonusCalcListener.Tests.UseCase
{
    public class BonusCalcTestDataFactory
    {
        public static Timesheet ValidTimesheet()
        {
            var trade = new BonusCalcListener.Infrastructure.Trade
            {
                Id = "EL",
                Description = "Electrician"
            };

            var scheme = new Scheme
            {
                Id = 1,
                Type = "SMV",
                Description = "Reactive",
                ConversionFactor = 1.0M
            };

            var operative = new Operative
            {
                Id = "123456",
                Name = "An Operative",
                Trade = trade,
                Scheme = scheme,
                Section = "H3007",
                SalaryBand = 5,
                FixedBand = false,
                IsArchived = false
            };

            var bonusPeriod = new BonusPeriod
            {
                Id = "2021-08-02",
                StartAt = new DateTime(2021, 8, 1, 23, 0, 0, DateTimeKind.Utc),
                Year = 2020,
                Number = 3,
                ClosedAt = null
            };

            var week = new Week
            {
                Id = "2021-11-08",
                BonusPeriod = bonusPeriod,
                StartAt = new DateTime(2021, 11, 07, 23, 0, 0, DateTimeKind.Utc),
                Number = 15,
                ClosedAt = null
            };

            var nonProductivePayElementType = new PayElementType
            {
                Id = 101,
                Description = "Dayworks",
                PayAtBand = false,
                Paid = true,
                Adjustment = false,
                Productive = false,
                NonProductive = true
            };

            var productivePayElementType = new PayElementType
            {
                Id = 301,
                Description = "Reactive Repairs",
                PayAtBand = true,
                Paid = true,
                Adjustment = false,
                Productive = true,
                NonProductive = false
            };

            var timesheet = new Timesheet
            {
                Week = week,
                Operative = operative,
                PayElements = GeneratePayElements().ToList()
            };

            return timesheet;
        }

        public static List<PayElement> GeneratePayElements()
        {
            var productivePayElementType = new PayElementType
            {
                Id = 301,
                Description = "Reactive Repairs",
                PayAtBand = true,
                Paid = true,
                Adjustment = false,
                Productive = true,
                NonProductive = false
            };

            var elements = new List<PayElement>
            {
                    new PayElement
                    {
                        PayElementType = productivePayElementType,
                        Monday = 0.0M,
                        Tuesday = 0.0M,
                        Wednesday = 7.0M,
                        Thursday = 0.0M,
                        Friday = 0.0M,
                        Duration = 7.0M,
                        Value = 0.0M,
                        ReadOnly = true,
                        WorkOrder = "10003773"
                    },
                    new PayElement
                    {
                        PayElementType = productivePayElementType,
                        Monday = 0.0M,
                        Tuesday = 7.0M,
                        Wednesday = 0.0M,
                        Thursday = 0.0M,
                        Friday = 0.0M,
                        Duration = 7.0M,
                        Value = 0.0M,
                        ReadOnly = true,
                        WorkOrder = "10003773"
                    }
            };

            return elements;
        }

        public static EntityEventSns ValidMessage()
        {
            return new EntityEventSns
            {
                Id = Guid.NewGuid(),
                EventType = RepairsEventTypes.WorkOrderUpdatedEvent,
                DateTime = DateTime.Now,
                EventData = new WorkOrderOperativeSmvData
                {
                    WorkOrderId = "10003773",
                    WorkOrderStatusCode = 50,
                    Address = "34 DeBeauvoir Square, London, N4 2FL",
                    StandardMinuteValue = 100,
                    OperativePrn = "4044",
                    JobPercentage = 0.50d,
                    ClosedTime = new DateTime(2021, 11, 09, 21, 28, 00)
                }
            };
        }

        public static EntityEventSns CancelledMessage()
        {
            return new EntityEventSns
            {
                Id = Guid.NewGuid(),
                EventType = RepairsEventTypes.WorkOrderUpdatedEvent,
                DateTime = DateTime.Now,
                EventData = new WorkOrderOperativeSmvData
                {
                    WorkOrderId = "10003773",
                    WorkOrderStatusCode = 30,
                    Address = "34 DeBeauvoir Square, London, N4 2FL",
                    StandardMinuteValue = 100,
                    OperativePrn = "4044",
                    JobPercentage = 0.50d,
                    ClosedTime = new DateTime(2021, 11, 09, 21, 28, 00)
                }
            };
        }
    }
}
