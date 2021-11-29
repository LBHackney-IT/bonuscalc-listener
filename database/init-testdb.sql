CREATE ROLE rds_replication WITH
  NOLOGIN
  NOSUPERUSER
  INHERIT
  NOCREATEDB
  NOCREATEROLE
  NOREPLICATION;

CREATE ROLE rds_password WITH
  NOLOGIN
  NOSUPERUSER
  INHERIT
  NOCREATEDB
  NOCREATEROLE
  NOREPLICATION;

CREATE ROLE rds_superuser WITH
  NOLOGIN
  NOSUPERUSER
  INHERIT
  NOCREATEDB
  NOCREATEROLE
  NOREPLICATION;

GRANT pg_monitor, pg_signal_backend, rds_password, rds_replication TO rds_superuser WITH ADMIN OPTION;

CREATE ROLE bonuscalc WITH
  LOGIN
  NOSUPERUSER
  INHERIT
  CREATEDB
  CREATEROLE
  NOREPLICATION
  VALID UNTIL 'infinity';

GRANT rds_superuser TO bonuscalc;

CREATE TABLE IF NOT EXISTS public.bonus_periods
(
    start_at timestamp without time zone NOT NULL,
    year integer NOT NULL,
    "number" integer NOT NULL,
    closed_at timestamp without time zone,
    id text COLLATE pg_catalog."default" NOT NULL DEFAULT ''::text,
    CONSTRAINT pk_bonus_periods PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE public.bonus_periods
    OWNER to bonuscalc;
-- Index: ix_bonus_periods_start_at

-- DROP INDEX public.ix_bonus_periods_start_at;

CREATE UNIQUE INDEX ix_bonus_periods_start_at
    ON public.bonus_periods USING btree
    (start_at ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: ix_bonus_periods_year_number

-- DROP INDEX public.ix_bonus_periods_year_number;

CREATE UNIQUE INDEX ix_bonus_periods_year_number
    ON public.bonus_periods USING btree
    (year ASC NULLS LAST, number ASC NULLS LAST)
    TABLESPACE pg_default;

CREATE TABLE IF NOT EXISTS public.trades
(
    id character varying(3) COLLATE pg_catalog."default" NOT NULL,
    description character varying(100) COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT pk_trades PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE public.trades
    OWNER to bonuscalc;
-- Index: ix_trades_description

-- DROP INDEX public.ix_trades_description;

CREATE UNIQUE INDEX ix_trades_description
    ON public.trades USING btree
    (description COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;

CREATE TABLE IF NOT EXISTS public.weeks
(
    bonus_period_id text COLLATE pg_catalog."default",
    start_at timestamp without time zone NOT NULL,
    "number" integer NOT NULL,
    closed_at timestamp without time zone,
    id text COLLATE pg_catalog."default" NOT NULL DEFAULT ''::text,
    CONSTRAINT pk_weeks PRIMARY KEY (id),
    CONSTRAINT fk_weeks_bonus_periods_bonus_period_id FOREIGN KEY (bonus_period_id)
        REFERENCES public.bonus_periods (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE RESTRICT
)

TABLESPACE pg_default;

ALTER TABLE public.weeks
    OWNER to bonuscalc;
-- Index: ix_weeks_bonus_period_id_number

-- DROP INDEX public.ix_weeks_bonus_period_id_number;

CREATE UNIQUE INDEX ix_weeks_bonus_period_id_number
    ON public.weeks USING btree
    (bonus_period_id COLLATE pg_catalog."default" ASC NULLS LAST, number ASC NULLS LAST)
    TABLESPACE pg_default;


CREATE TABLE IF NOT EXISTS public.pay_element_types
(
    id integer NOT NULL,
    description character varying(100) COLLATE pg_catalog."default" NOT NULL,
    pay_at_band boolean NOT NULL,
    paid boolean NOT NULL,
    adjustment boolean NOT NULL DEFAULT false,
    productive boolean NOT NULL DEFAULT false,
    non_productive boolean NOT NULL DEFAULT false,
    out_of_hours boolean NOT NULL DEFAULT false,
    overtime boolean NOT NULL DEFAULT false,
    selectable boolean NOT NULL DEFAULT false,
    CONSTRAINT pk_pay_element_types PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE public.pay_element_types
    OWNER to bonuscalc;
-- Index: ix_pay_element_types_description

-- DROP INDEX public.ix_pay_element_types_description;

CREATE UNIQUE INDEX ix_pay_element_types_description
    ON public.pay_element_types USING btree
    (description COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;

CREATE TABLE IF NOT EXISTS public.schemes
(
    id integer NOT NULL,
    type character varying(10) COLLATE pg_catalog."default" NOT NULL,
    description character varying(100) COLLATE pg_catalog."default" NOT NULL,
    conversion_factor numeric(20,14) NOT NULL DEFAULT 1.0,
    CONSTRAINT pk_schemes PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE public.schemes
    OWNER to bonuscalc;
-- Index: ix_schemes_description

-- DROP INDEX public.ix_schemes_description;

CREATE UNIQUE INDEX ix_schemes_description
    ON public.schemes USING btree
    (description COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;

CREATE TABLE IF NOT EXISTS public.operatives
(
    id character varying(6) COLLATE pg_catalog."default" NOT NULL,
    name character varying(100) COLLATE pg_catalog."default" NOT NULL,
    trade_id character varying(3) COLLATE pg_catalog."default" NOT NULL,
    section character varying(10) COLLATE pg_catalog."default" NOT NULL,
    salary_band integer NOT NULL,
    fixed_band boolean NOT NULL,
    is_archived boolean NOT NULL,
    scheme_id integer,
    CONSTRAINT pk_operatives PRIMARY KEY (id),
    CONSTRAINT fk_operatives_schemes_scheme_id FOREIGN KEY (scheme_id)
        REFERENCES public.schemes (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE RESTRICT,
    CONSTRAINT fk_operatives_trades_trade_id FOREIGN KEY (trade_id)
        REFERENCES public.trades (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)

TABLESPACE pg_default;

ALTER TABLE public.operatives
    OWNER to bonuscalc;
-- Index: ix_operatives_scheme_id

-- DROP INDEX public.ix_operatives_scheme_id;

CREATE INDEX ix_operatives_scheme_id
    ON public.operatives USING btree
    (scheme_id ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: ix_operatives_trade_id

-- DROP INDEX public.ix_operatives_trade_id;

CREATE INDEX ix_operatives_trade_id
    ON public.operatives USING btree
    (trade_id COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;

CREATE TABLE IF NOT EXISTS public.pay_bands
(
    id integer NOT NULL,
    band integer NOT NULL,
    value numeric NOT NULL,
    scheme_id integer,
    CONSTRAINT pk_pay_bands PRIMARY KEY (id),
    CONSTRAINT fk_pay_bands_schemes_scheme_id FOREIGN KEY (scheme_id)
        REFERENCES public.schemes (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE RESTRICT
)

TABLESPACE pg_default;

ALTER TABLE public.pay_bands
    OWNER to bonuscalc;
-- Index: ix_pay_bands_scheme_id

-- DROP INDEX public.ix_pay_bands_scheme_id;

CREATE INDEX ix_pay_bands_scheme_id
    ON public.pay_bands USING btree
    (scheme_id ASC NULLS LAST)
    TABLESPACE pg_default;

CREATE TABLE IF NOT EXISTS public.timesheets
(
    id integer NOT NULL GENERATED BY DEFAULT AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
    operative_id character varying(6) COLLATE pg_catalog."default" NOT NULL,
    week_id text COLLATE pg_catalog."default",
    CONSTRAINT pk_timesheets PRIMARY KEY (id),
    CONSTRAINT fk_timesheets_operatives_operative_id FOREIGN KEY (operative_id)
        REFERENCES public.operatives (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
    CONSTRAINT fk_timesheets_weeks_week_id FOREIGN KEY (week_id)
        REFERENCES public.weeks (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE RESTRICT
)

TABLESPACE pg_default;

ALTER TABLE public.timesheets
    OWNER to bonuscalc;
-- Index: ix_timesheets_operative_id_week_id

-- DROP INDEX public.ix_timesheets_operative_id_week_id;

CREATE UNIQUE INDEX ix_timesheets_operative_id_week_id
    ON public.timesheets USING btree
    (operative_id COLLATE pg_catalog."default" ASC NULLS LAST, week_id COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: ix_timesheets_week_id

-- DROP INDEX public.ix_timesheets_week_id;

CREATE INDEX ix_timesheets_week_id
    ON public.timesheets USING btree
    (week_id COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;

CREATE TABLE IF NOT EXISTS public.pay_elements
(
    id integer NOT NULL GENERATED BY DEFAULT AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
    timesheet_id integer NOT NULL,
    pay_element_type_id integer NOT NULL,
    work_order character varying(10) COLLATE pg_catalog."default",
    address text COLLATE pg_catalog."default",
    comment text COLLATE pg_catalog."default",
    duration numeric(10,4) NOT NULL,
    value numeric(10,4) NOT NULL,
    read_only boolean NOT NULL DEFAULT false,
    friday numeric(10,4) NOT NULL DEFAULT 0.0,
    monday numeric(10,4) NOT NULL DEFAULT 0.0,
    saturday numeric(10,4) NOT NULL DEFAULT 0.0,
    sunday numeric(10,4) NOT NULL DEFAULT 0.0,
    thursday numeric(10,4) NOT NULL DEFAULT 0.0,
    tuesday numeric(10,4) NOT NULL DEFAULT 0.0,
    wednesday numeric(10,4) NOT NULL DEFAULT 0.0,
    closed_at timestamp without time zone,
    CONSTRAINT pk_pay_elements PRIMARY KEY (id),
    CONSTRAINT fk_pay_elements_pay_element_types_pay_element_type_id FOREIGN KEY (pay_element_type_id)
        REFERENCES public.pay_element_types (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
    CONSTRAINT fk_pay_elements_timesheets_timesheet_id FOREIGN KEY (timesheet_id)
        REFERENCES public.timesheets (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)

TABLESPACE pg_default;

ALTER TABLE public.pay_elements
    OWNER to bonuscalc;
-- Index: ix_pay_elements_pay_element_type_id

-- DROP INDEX public.ix_pay_elements_pay_element_type_id;

CREATE INDEX ix_pay_elements_pay_element_type_id
    ON public.pay_elements USING btree
    (pay_element_type_id ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: ix_pay_elements_timesheet_id

-- DROP INDEX public.ix_pay_elements_timesheet_id;

CREATE INDEX ix_pay_elements_timesheet_id
    ON public.pay_elements USING btree
    (timesheet_id ASC NULLS LAST)
    TABLESPACE pg_default;