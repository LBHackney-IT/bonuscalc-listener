--
-- PostgreSQL database dump
--

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL
);


--
-- Name: bonus_periods; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.bonus_periods (
    start_at timestamp without time zone NOT NULL,
    year integer NOT NULL,
    number integer NOT NULL,
    closed_at timestamp without time zone,
    id character varying(10) NOT NULL
);


--
-- Name: pay_element_types; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.pay_element_types (
    id integer NOT NULL,
    description character varying(100) NOT NULL,
    pay_at_band boolean NOT NULL,
    paid boolean NOT NULL,
    adjustment boolean DEFAULT false NOT NULL,
    productive boolean DEFAULT false NOT NULL,
    non_productive boolean DEFAULT false NOT NULL,
    out_of_hours boolean DEFAULT false NOT NULL,
    overtime boolean DEFAULT false NOT NULL,
    selectable boolean DEFAULT false NOT NULL,
    smv_per_hour integer
);


--
-- Name: pay_elements; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.pay_elements (
    id integer NOT NULL,
    pay_element_type_id integer NOT NULL,
    work_order character varying(10),
    address text,
    comment text,
    duration numeric(10,4) NOT NULL,
    value numeric(10,4) NOT NULL,
    read_only boolean DEFAULT false NOT NULL,
    friday numeric(10,4) DEFAULT 0.0 NOT NULL,
    monday numeric(10,4) DEFAULT 0.0 NOT NULL,
    saturday numeric(10,4) DEFAULT 0.0 NOT NULL,
    sunday numeric(10,4) DEFAULT 0.0 NOT NULL,
    thursday numeric(10,4) DEFAULT 0.0 NOT NULL,
    tuesday numeric(10,4) DEFAULT 0.0 NOT NULL,
    wednesday numeric(10,4) DEFAULT 0.0 NOT NULL,
    timesheet_id character varying(17) NOT NULL,
    closed_at timestamp without time zone,
    search_vector tsvector GENERATED ALWAYS AS (to_tsvector('simple'::regconfig, (((COALESCE(work_order, ''::character varying))::text || ' '::text) || COALESCE(address, ''::text)))) STORED,
    cost_code character varying(5)
);


--
-- Name: non_productive_pay_elements; Type: VIEW; Schema: public; Owner: -
--

CREATE VIEW public.non_productive_pay_elements AS
 SELECT p.timesheet_id,
    (sum(p.duration))::numeric(10,4) AS duration,
    (sum(p.value))::numeric(10,4) AS value
   FROM (public.pay_elements p
     JOIN public.pay_element_types t ON ((p.pay_element_type_id = t.id)))
  WHERE (t.non_productive = true)
  GROUP BY p.timesheet_id;


--
-- Name: operatives; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.operatives (
    id character varying(6) NOT NULL,
    name character varying(100) NOT NULL,
    trade_id character varying(3) NOT NULL,
    section character varying(10) NOT NULL,
    salary_band integer NOT NULL,
    fixed_band boolean NOT NULL,
    is_archived boolean NOT NULL,
    scheme_id integer,
    email_address character varying(100),
    utilisation numeric(5,4) DEFAULT 1.0 NOT NULL,
    search_vector tsvector GENERATED ALWAYS AS (to_tsvector('simple'::regconfig, (((((((id)::text || ' '::text) || (name)::text) || ' '::text) || (trade_id)::text) || ' '::text) || (section)::text))) STORED
);


--
-- Name: productive_pay_elements; Type: VIEW; Schema: public; Owner: -
--

CREATE VIEW public.productive_pay_elements AS
 SELECT p.timesheet_id,
    (sum(p.value))::numeric(10,4) AS value
   FROM (public.pay_elements p
     JOIN public.pay_element_types t ON ((p.pay_element_type_id = t.id)))
  WHERE (t.productive = true)
  GROUP BY p.timesheet_id;


--
-- Name: timesheets; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.timesheets (
    operative_id character varying(6) NOT NULL,
    week_id character varying(10) NOT NULL,
    id character varying(17) NOT NULL,
    utilisation numeric(5,4) DEFAULT 1.0 NOT NULL,
    report_sent_at timestamp without time zone
);


--
-- Name: trades; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.trades (
    id character varying(3) NOT NULL,
    description character varying(100) NOT NULL
);


--
-- Name: weeks; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.weeks (
    bonus_period_id character varying(10) NOT NULL,
    start_at timestamp without time zone NOT NULL,
    number integer NOT NULL,
    closed_at timestamp without time zone,
    id character varying(10) NOT NULL,
    closed_by character varying(100),
    reports_sent_at timestamp without time zone
);


--
-- Name: weekly_summaries; Type: VIEW; Schema: public; Owner: -
--

CREATE VIEW public.weekly_summaries AS
 SELECT concat(t.operative_id, '/', w.bonus_period_id, '/', w.id) AS id,
    concat(t.operative_id, '/', w.bonus_period_id) AS summary_id,
    w.id AS week_id,
    t.operative_id,
    w.number,
    w.start_at,
    w.closed_at,
    (COALESCE(p.value, (0)::numeric))::numeric(10,4) AS productive_value,
    (COALESCE(np.duration, (0)::numeric))::numeric(10,4) AS non_productive_duration,
    (COALESCE(np.value, (0)::numeric))::numeric(10,4) AS non_productive_value,
    ((COALESCE(p.value, (0)::numeric) + COALESCE(np.value, (0)::numeric)))::numeric(10,4) AS total_value,
    t.utilisation,
    (round(avg((COALESCE(p.value, (0)::numeric) + COALESCE(np.value, (0)::numeric))) OVER (PARTITION BY w.bonus_period_id, t.operative_id ORDER BY w.number), 4))::numeric(10,4) AS projected_value,
    (round(avg(t.utilisation) OVER (PARTITION BY w.bonus_period_id, t.operative_id ORDER BY w.number), 4))::numeric(5,4) AS average_utilisation,
    t.report_sent_at
   FROM (((public.weeks w
     JOIN public.timesheets t ON (((w.id)::text = (t.week_id)::text)))
     LEFT JOIN public.productive_pay_elements p ON (((t.id)::text = (p.timesheet_id)::text)))
     LEFT JOIN public.non_productive_pay_elements np ON (((t.id)::text = (np.timesheet_id)::text)));


--
-- Name: operative_summaries; Type: VIEW; Schema: public; Owner: -
--

CREATE VIEW public.operative_summaries AS
 SELECT ws.operative_id AS id,
    ws.week_id,
    o.name,
    o.trade_id,
    t.description AS trade_description,
    o.scheme_id,
    ws.productive_value,
    ws.non_productive_duration,
    ws.non_productive_value,
    ws.total_value,
    ws.utilisation,
    ws.projected_value,
    ws.average_utilisation,
    ws.report_sent_at
   FROM ((public.weekly_summaries ws
     JOIN public.operatives o ON (((ws.operative_id)::text = (o.id)::text)))
     JOIN public.trades t ON (((o.trade_id)::text = (t.id)::text)));


--
-- Name: pay_bands; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.pay_bands (
    id integer NOT NULL,
    band integer NOT NULL,
    value numeric NOT NULL,
    scheme_id integer
);


--
-- Name: pay_elements_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.pay_elements ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.pay_elements_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: schemes; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.schemes (
    id integer NOT NULL,
    type character varying(10) NOT NULL,
    description character varying(100) NOT NULL,
    conversion_factor numeric(20,14) DEFAULT 1.0 NOT NULL
);


--
-- Name: summaries; Type: VIEW; Schema: public; Owner: -
--

CREATE VIEW public.summaries AS
 SELECT concat(t.operative_id, '/', w.bonus_period_id) AS id,
    t.operative_id,
    w.bonus_period_id
   FROM (public.timesheets t
     JOIN public.weeks w ON (((t.week_id)::text = (w.id)::text)))
  GROUP BY t.operative_id, w.bonus_period_id;


--
-- Name: work_elements; Type: VIEW; Schema: public; Owner: -
--

CREATE VIEW public.work_elements AS
 SELECT pe.id,
    pe.pay_element_type_id,
    pe.work_order,
    pe.address,
    pe.comment AS description,
    t.operative_id,
    o.name AS operative_name,
    t.week_id,
    pe.value,
    pe.closed_at,
    pe.search_vector
   FROM ((public.pay_elements pe
     JOIN public.timesheets t ON (((pe.timesheet_id)::text = (t.id)::text)))
     JOIN public.operatives o ON (((t.operative_id)::text = (o.id)::text)));


--
-- Name: __EFMigrationsHistory pk___ef_migrations_history; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id);


--
-- Name: bonus_periods pk_bonus_periods; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.bonus_periods
    ADD CONSTRAINT pk_bonus_periods PRIMARY KEY (id);


--
-- Name: operatives pk_operatives; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.operatives
    ADD CONSTRAINT pk_operatives PRIMARY KEY (id);


--
-- Name: pay_bands pk_pay_bands; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pay_bands
    ADD CONSTRAINT pk_pay_bands PRIMARY KEY (id);


--
-- Name: pay_element_types pk_pay_element_types; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pay_element_types
    ADD CONSTRAINT pk_pay_element_types PRIMARY KEY (id);


--
-- Name: pay_elements pk_pay_elements; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pay_elements
    ADD CONSTRAINT pk_pay_elements PRIMARY KEY (id);


--
-- Name: schemes pk_schemes; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.schemes
    ADD CONSTRAINT pk_schemes PRIMARY KEY (id);


--
-- Name: timesheets pk_timesheets; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.timesheets
    ADD CONSTRAINT pk_timesheets PRIMARY KEY (id);


--
-- Name: trades pk_trades; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.trades
    ADD CONSTRAINT pk_trades PRIMARY KEY (id);


--
-- Name: weeks pk_weeks; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.weeks
    ADD CONSTRAINT pk_weeks PRIMARY KEY (id);


--
-- Name: ix_bonus_periods_start_at; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_bonus_periods_start_at ON public.bonus_periods USING btree (start_at);


--
-- Name: ix_bonus_periods_year_number; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_bonus_periods_year_number ON public.bonus_periods USING btree (year, number);


--
-- Name: ix_operatives_email_address; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_operatives_email_address ON public.operatives USING btree (email_address);


--
-- Name: ix_operatives_scheme_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_operatives_scheme_id ON public.operatives USING btree (scheme_id);


--
-- Name: ix_operatives_search_vector; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_operatives_search_vector ON public.operatives USING gin (search_vector);


--
-- Name: ix_operatives_trade_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_operatives_trade_id ON public.operatives USING btree (trade_id);


--
-- Name: ix_pay_bands_scheme_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_pay_bands_scheme_id ON public.pay_bands USING btree (scheme_id);


--
-- Name: ix_pay_element_types_description; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_pay_element_types_description ON public.pay_element_types USING btree (description);


--
-- Name: ix_pay_elements_pay_element_type_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_pay_elements_pay_element_type_id ON public.pay_elements USING btree (pay_element_type_id);


--
-- Name: ix_pay_elements_search_vector; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_pay_elements_search_vector ON public.pay_elements USING gin (search_vector);


--
-- Name: ix_pay_elements_timesheet_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_pay_elements_timesheet_id ON public.pay_elements USING btree (timesheet_id);


--
-- Name: ix_pay_elements_cost_code; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_pay_elements_cost_code ON public.pay_elements USING btree (cost_code);


--
-- Name: ix_schemes_description; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_schemes_description ON public.schemes USING btree (description);


--
-- Name: ix_timesheets_operative_id_week_id; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_timesheets_operative_id_week_id ON public.timesheets USING btree (operative_id, week_id);


--
-- Name: ix_timesheets_week_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_timesheets_week_id ON public.timesheets USING btree (week_id);


--
-- Name: ix_trades_description; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_trades_description ON public.trades USING btree (description);


--
-- Name: ix_weeks_bonus_period_id_number; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_weeks_bonus_period_id_number ON public.weeks USING btree (bonus_period_id, number);


--
-- Name: operatives fk_operatives_schemes_scheme_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.operatives
    ADD CONSTRAINT fk_operatives_schemes_scheme_id FOREIGN KEY (scheme_id) REFERENCES public.schemes(id) ON DELETE RESTRICT;


--
-- Name: operatives fk_operatives_trades_trade_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.operatives
    ADD CONSTRAINT fk_operatives_trades_trade_id FOREIGN KEY (trade_id) REFERENCES public.trades(id) ON DELETE CASCADE;


--
-- Name: pay_bands fk_pay_bands_schemes_scheme_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pay_bands
    ADD CONSTRAINT fk_pay_bands_schemes_scheme_id FOREIGN KEY (scheme_id) REFERENCES public.schemes(id) ON DELETE RESTRICT;


--
-- Name: pay_elements fk_pay_elements_pay_element_types_pay_element_type_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pay_elements
    ADD CONSTRAINT fk_pay_elements_pay_element_types_pay_element_type_id FOREIGN KEY (pay_element_type_id) REFERENCES public.pay_element_types(id) ON DELETE CASCADE;


--
-- Name: pay_elements fk_pay_elements_timesheets_timesheet_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pay_elements
    ADD CONSTRAINT fk_pay_elements_timesheets_timesheet_id FOREIGN KEY (timesheet_id) REFERENCES public.timesheets(id) ON DELETE RESTRICT;


--
-- Name: timesheets fk_timesheets_operatives_operative_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.timesheets
    ADD CONSTRAINT fk_timesheets_operatives_operative_id FOREIGN KEY (operative_id) REFERENCES public.operatives(id) ON DELETE CASCADE;


--
-- Name: timesheets fk_timesheets_weeks_week_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.timesheets
    ADD CONSTRAINT fk_timesheets_weeks_week_id FOREIGN KEY (week_id) REFERENCES public.weeks(id) ON DELETE CASCADE;


--
-- Name: weeks fk_weeks_bonus_periods_bonus_period_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.weeks
    ADD CONSTRAINT fk_weeks_bonus_periods_bonus_period_id FOREIGN KEY (bonus_period_id) REFERENCES public.bonus_periods(id) ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--
