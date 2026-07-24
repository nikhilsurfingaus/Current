--
-- PostgreSQL database dump
--

\restrict p8Gugexui4mvAivWd34C1fQGFxJXZZIpyA2n1QCTa0KBGJTOdaji2sShgetoFEc

-- Dumped from database version 17.10 (Homebrew)
-- Dumped by pg_dump version 17.10 (Homebrew)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
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
-- Name: Accounts; Type: TABLE; Schema: public; Owner: nikhil
--

CREATE TABLE public."Accounts" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "AccountType" character varying(50) NOT NULL,
    "CurrentBalance" numeric(18,2) NOT NULL,
    "Currency" character varying(3) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public."Accounts" OWNER TO nikhil;

--
-- Name: Branches; Type: TABLE; Schema: public; Owner: nikhil
--

CREATE TABLE public."Branches" (
    "Id" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Code" character varying(20) NOT NULL,
    "TreasuryAccountId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public."Branches" OWNER TO nikhil;

--
-- Name: Contacts; Type: TABLE; Schema: public; Owner: nikhil
--

CREATE TABLE public."Contacts" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Email" character varying(255) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public."Contacts" OWNER TO nikhil;

--
-- Name: GoalContributions; Type: TABLE; Schema: public; Owner: nikhil
--

CREATE TABLE public."GoalContributions" (
    "Id" uuid NOT NULL,
    "GoalId" uuid NOT NULL,
    "TransactionId" uuid,
    "Amount" numeric(18,2) NOT NULL,
    "ContributionType" character varying(50) NOT NULL,
    "Note" character varying(500),
    "CreatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public."GoalContributions" OWNER TO nikhil;

--
-- Name: Goals; Type: TABLE; Schema: public; Owner: nikhil
--

CREATE TABLE public."Goals" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "SourceAccountId" uuid NOT NULL,
    "GoalAccountId" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Description" character varying(500),
    "TargetAmount" numeric(18,2) NOT NULL,
    "CurrentAmount" numeric(18,2) NOT NULL,
    "Currency" character varying(3) NOT NULL,
    "TargetDate" date,
    "Status" character varying(50) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "IconKey" character varying(50) DEFAULT ''::character varying NOT NULL
);


ALTER TABLE public."Goals" OWNER TO nikhil;

--
-- Name: IdempotencyKeys; Type: TABLE; Schema: public; Owner: nikhil
--

CREATE TABLE public."IdempotencyKeys" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Key" character varying(100) NOT NULL,
    "RequestHash" character varying(64) NOT NULL,
    "ResponseJson" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "ExpiresAt" timestamp with time zone NOT NULL
);


ALTER TABLE public."IdempotencyKeys" OWNER TO nikhil;

--
-- Name: LedgerEntries; Type: TABLE; Schema: public; Owner: nikhil
--

CREATE TABLE public."LedgerEntries" (
    "Id" uuid NOT NULL,
    "TransactionId" uuid NOT NULL,
    "AccountId" uuid NOT NULL,
    "EntryType" character varying(50) NOT NULL,
    "Amount" numeric(18,2) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public."LedgerEntries" OWNER TO nikhil;

--
-- Name: LoanRepayments; Type: TABLE; Schema: public; Owner: nikhil
--

CREATE TABLE public."LoanRepayments" (
    "Id" uuid NOT NULL,
    "LoanId" uuid NOT NULL,
    "TransactionId" uuid,
    "Amount" numeric(18,2) NOT NULL,
    "PrincipalPortion" numeric(18,2) NOT NULL,
    "InterestPortion" numeric(18,2) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public."LoanRepayments" OWNER TO nikhil;

--
-- Name: Loans; Type: TABLE; Schema: public; Owner: nikhil
--

CREATE TABLE public."Loans" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "BranchId" uuid NOT NULL,
    "FundedAccountId" uuid NOT NULL,
    "DisbursementTransactionId" uuid,
    "Principal" numeric(18,2) NOT NULL,
    "OutstandingPrincipal" numeric(18,2) NOT NULL,
    "InterestRatePercent" numeric(8,4) NOT NULL,
    "MonthlyPayment" numeric(18,2) NOT NULL,
    "Currency" character varying(3) NOT NULL,
    "TermMonths" integer NOT NULL,
    "StartDate" date,
    "NextDueDate" date,
    "MaturityDate" date,
    "Status" character varying(50) NOT NULL,
    "Purpose" character varying(500),
    "RejectionReason" character varying(500),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public."Loans" OWNER TO nikhil;

--
-- Name: Notifications; Type: TABLE; Schema: public; Owner: nikhil
--

CREATE TABLE public."Notifications" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Title" character varying(200) NOT NULL,
    "Body" character varying(1000) NOT NULL,
    "NotificationType" character varying(50) NOT NULL,
    "IsRead" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "RelatedEntityId" uuid
);


ALTER TABLE public."Notifications" OWNER TO nikhil;

--
-- Name: Transactions; Type: TABLE; Schema: public; Owner: nikhil
--

CREATE TABLE public."Transactions" (
    "Id" uuid NOT NULL,
    "FromAccountId" uuid NOT NULL,
    "ToAccountId" uuid NOT NULL,
    "Amount" numeric(18,2) NOT NULL,
    "Description" character varying(500) NOT NULL,
    "Status" character varying(50) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "Category" character varying(50) DEFAULT ''::character varying NOT NULL,
    "Merchant" character varying(200),
    "Reference" character varying(100)
);


ALTER TABLE public."Transactions" OWNER TO nikhil;

--
-- Name: Users; Type: TABLE; Schema: public; Owner: nikhil
--

CREATE TABLE public."Users" (
    "Id" uuid NOT NULL,
    "FirstName" character varying(100) NOT NULL,
    "LastName" character varying(100) NOT NULL,
    "Email" character varying(255) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "PasswordHash" character varying(500) DEFAULT ''::character varying NOT NULL,
    "Role" character varying(50) DEFAULT 'User'::character varying NOT NULL,
    "Locale" character varying(20) DEFAULT 'en-AU'::character varying NOT NULL,
    "PreferredCurrency" character varying(3) DEFAULT 'AUD'::character varying NOT NULL,
    "ThemePreference" character varying(20) DEFAULT 'System'::character varying NOT NULL,
    "Timezone" character varying(100) DEFAULT 'Australia/Sydney'::character varying NOT NULL
);


ALTER TABLE public."Users" OWNER TO nikhil;

--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: nikhil
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


ALTER TABLE public."__EFMigrationsHistory" OWNER TO nikhil;

--
-- Data for Name: Accounts; Type: TABLE DATA; Schema: public; Owner: nikhil
--

COPY public."Accounts" ("Id", "UserId", "Name", "AccountType", "CurrentBalance", "Currency", "CreatedAt", "UpdatedAt") FROM stdin;
cc0afab3-d4c6-4d85-abbd-5033ce2c24d1	04d20afb-e7eb-488c-814e-3c865a3c38ea	Euro Summer	Savings	6900.00	AUD	2026-07-01 21:37:33.81699+10	2026-07-03 02:29:16.209261+10
2e9d6587-1940-4ba5-b95b-2cdd31507e30	04d20afb-e7eb-488c-814e-3c865a3c38ea	Bills	Everyday	11134.67	AUD	2026-07-01 21:38:09.236162+10	2026-07-23 15:39:24.078121+10
5b0fbd99-69c9-435b-81f5-6c931654fcee	79483998-193a-4b74-b8a6-a3c98e3131e2	Gold Coast House	Savings	102650.00	AUD	2026-07-01 21:34:36.829337+10	2026-07-23 15:39:24.078121+10
7f319585-0d71-49e2-bbbb-282e9c3c971c	79483998-193a-4b74-b8a6-a3c98e3131e2	Caravan Park Holiday	Savings	2000.00	AUD	2026-07-05 21:59:04.582714+10	2026-07-05 21:59:12.637172+10
b45d4b83-54df-45ea-867c-86c34b842421	79483998-193a-4b74-b8a6-a3c98e3131e2	Gold Coast Property	Savings	399931.00	AUD	2026-07-05 21:53:19.89351+10	2026-07-16 23:09:04.404661+10
c745c7d8-e1a3-4f27-90cd-6413190b9cac	79483998-193a-4b74-b8a6-a3c98e3131e2	NVIDIA Profit	Investment	40000.00	AUD	2026-07-01 21:35:35.065366+10	2026-07-19 20:15:51.565197+10
5fa23e09-4221-4ccb-bbf9-ae68b73f58f1	8303abc0-f937-443b-a4d4-3e60943bd482	Main	Everyday	179975.00	AUD	2026-07-19 20:28:28.208815+10	2026-07-19 20:35:02.413946+10
4136fc78-722d-409e-82f6-1c8d8dc57a41	79483998-193a-4b74-b8a6-a3c98e3131e2	Crypto Investment Funds	Investment	16000.00	AUD	2026-07-05 20:29:43.797084+10	2026-07-19 20:37:13.535918+10
38b99e28-08f2-4038-9a42-eb4987d824a0	79483998-193a-4b74-b8a6-a3c98e3131e2	Prado J120	Savings	25000.00	AUD	2026-07-05 21:52:22.309758+10	2026-07-22 21:41:20.641331+10
22222222-2222-2222-2222-222222222222	11111111-1111-1111-1111-111111111111	Current HQ Treasury	Branch	9935299.33	AUD	2026-07-22 19:40:51+10	2026-07-22 21:45:17.641752+10
9086bb3b-e80b-4aa7-a788-f5f71d798657	79483998-193a-4b74-b8a6-a3c98e3131e2	Main	Everyday	42010.00	AUD	2026-07-22 21:09:44.215432+10	2026-07-22 21:45:17.641752+10
\.


--
-- Data for Name: Branches; Type: TABLE DATA; Schema: public; Owner: nikhil
--

COPY public."Branches" ("Id", "Name", "Code", "TreasuryAccountId", "CreatedAt", "UpdatedAt") FROM stdin;
33333333-3333-3333-3333-333333333333	Current HQ	HQ	22222222-2222-2222-2222-222222222222	2026-07-22 19:40:51+10	2026-07-22 19:40:51+10
\.


--
-- Data for Name: Contacts; Type: TABLE DATA; Schema: public; Owner: nikhil
--

COPY public."Contacts" ("Id", "UserId", "Name", "Email", "CreatedAt", "UpdatedAt") FROM stdin;
6bbfeba1-458f-47af-afc0-0b2a4ad1438f	79483998-193a-4b74-b8a6-a3c98e3131e2	Mirabel	cobrastormgaming@gmail.com	2026-07-19 20:14:15.558254+10	2026-07-19 20:14:15.558254+10
90212c2e-c67a-45f3-8f48-75219b4859cd	04d20afb-e7eb-488c-814e-3c865a3c38ea	Nikhil Naik	nikhilsurfingaus@gmail.com	2026-07-19 20:19:37.537301+10	2026-07-19 20:19:37.537301+10
22951f4f-bfa8-4b6c-be27-59e8084ddf5b	8303abc0-f937-443b-a4d4-3e60943bd482	Nikhil Naik	nikhilsurfingaus@gmail.com	2026-07-19 20:35:02.422042+10	2026-07-19 20:35:02.422042+10
a2af60c0-eb4c-47f0-9c29-fc7d2d384380	79483998-193a-4b74-b8a6-a3c98e3131e2	Gordan Ramsey	aussiepilotlife@gmail.com	2026-07-19 20:35:18.187307+10	2026-07-19 20:35:18.187307+10
\.


--
-- Data for Name: GoalContributions; Type: TABLE DATA; Schema: public; Owner: nikhil
--

COPY public."GoalContributions" ("Id", "GoalId", "TransactionId", "Amount", "ContributionType", "Note", "CreatedAt") FROM stdin;
f022a063-7089-4b9d-b448-ef24c56d26b7	8030f562-9c36-49be-a7cd-861411428c45	4a85afcb-c059-46ec-9fe0-e21d6cdd8657	2000.00	Deposit	\N	2026-07-05 21:52:44.012231+10
2628dfc6-5d79-43c0-aab6-78008f36af95	12f904a0-33cf-4324-961e-945043229739	75ea72d5-118f-4862-bcd8-1317f0e42994	500000.00	Deposit	\N	2026-07-05 21:58:26.968129+10
f1118b58-aa5f-4df3-981d-7e82ea1be089	ef39e633-8a95-455e-993d-eefc4d8e24ff	1c88f6b4-84df-4319-a2d1-c50fe9df3b2c	2000.00	Deposit	\N	2026-07-05 21:59:12.637172+10
75c438af-b014-41cb-9da3-3bf417447e17	12f904a0-33cf-4324-961e-945043229739	8c56930b-d719-4c99-ace9-e555f92c2383	100000.00	Withdrawal	\N	2026-07-09 02:07:07.659668+10
b7d2d7b3-2fda-4246-98c9-39a561d1065a	8030f562-9c36-49be-a7cd-861411428c45	cbe99bf0-2efb-4753-bce2-93aa0aaf035a	8000.00	Deposit	Sold Subaru	2026-07-19 20:37:13.535918+10
\.


--
-- Data for Name: Goals; Type: TABLE DATA; Schema: public; Owner: nikhil
--

COPY public."Goals" ("Id", "UserId", "SourceAccountId", "GoalAccountId", "Name", "Description", "TargetAmount", "CurrentAmount", "Currency", "TargetDate", "Status", "CreatedAt", "UpdatedAt", "IconKey") FROM stdin;
ef39e633-8a95-455e-993d-eefc4d8e24ff	79483998-193a-4b74-b8a6-a3c98e3131e2	c745c7d8-e1a3-4f27-90cd-6413190b9cac	7f319585-0d71-49e2-bbbb-282e9c3c971c	Caravan Park Holiday	\N	2000.00	2000.00	AUD	2026-07-25	Completed	2026-07-05 21:59:04.582714+10	2026-07-05 21:59:12.637172+10	vacation
12f904a0-33cf-4324-961e-945043229739	79483998-193a-4b74-b8a6-a3c98e3131e2	5b0fbd99-69c9-435b-81f5-6c931654fcee	b45d4b83-54df-45ea-867c-86c34b842421	Gold Coast Property	\N	1600000.00	400000.00	AUD	2030-07-05	Active	2026-07-05 21:53:19.89351+10	2026-07-09 02:07:07.659668+10	default
8030f562-9c36-49be-a7cd-861411428c45	79483998-193a-4b74-b8a6-a3c98e3131e2	4136fc78-722d-409e-82f6-1c8d8dc57a41	38b99e28-08f2-4038-9a42-eb4987d824a0	Prado J120	2004 Toyota J120 Prado Landcruiser	16000.00	10000.00	AUD	2026-10-16	Active	2026-07-05 21:52:22.309758+10	2026-07-19 20:37:13.535918+10	car
\.


--
-- Data for Name: IdempotencyKeys; Type: TABLE DATA; Schema: public; Owner: nikhil
--

COPY public."IdempotencyKeys" ("Id", "UserId", "Key", "RequestHash", "ResponseJson", "CreatedAt", "ExpiresAt") FROM stdin;
c4662be6-67f0-4092-b304-27aa6ebcd2bb	79483998-193a-4b74-b8a6-a3c98e3131e2	11111111-1111-1111-1111-111111111111	066E7660C313943F79CD279F82C0A812682CA7BBE705C4B4B8CB6AE428AD61D3	{"transactionId":"ecdfd77f-7662-4ba0-bbfc-3559b9e048a6","fromAccountId":"b45d4b83-54df-45ea-867c-86c34b842421","recipientAccountId":"2e9d6587-1940-4ba5-b95b-2cdd31507e30","recipientAccountName":"Bills","recipientName":"Mirabel Suttcliffe","recipientEmail":"cobrastormgaming@gmail.com","amount":69,"currency":"AUD","reference":"Dinner xoxo","status":"Completed","createdAt":"2026-07-16T13:09:04.404661Z"}	2026-07-16 23:09:04.226543+10	2026-07-17 23:09:04.226543+10
377484cd-f243-41e7-ac2a-440194808e6f	79483998-193a-4b74-b8a6-a3c98e3131e2	1adb38b6-21af-4d71-be41-0688afdf4095	B0B33224A80532711C40F7D65EFDD8AE6779361E4EC8011CC4FFDE21B9F378D9	{"transactionId":"378092e8-194f-430e-9900-a53e8961e92d","fromAccountId":"c745c7d8-e1a3-4f27-90cd-6413190b9cac","recipientAccountId":"2e9d6587-1940-4ba5-b95b-2cdd31507e30","recipientAccountName":"Bills","recipientName":"Mirabel Suttcliffe","recipientEmail":"cobrastormgaming@gmail.com","amount":3000,"currency":"AUD","reference":"Mechanic Fees","status":"Completed","createdAt":"2026-07-19T10:15:51.565197Z"}	2026-07-19 20:15:51.4943+10	2026-07-20 20:15:51.4943+10
108e7ff2-2f53-406a-bd16-a8c119a0fed9	04d20afb-e7eb-488c-814e-3c865a3c38ea	7f52a417-93e9-4f33-bab5-af3c79a220ad	3F049472F3AE0F65413CEED459F87D385A55ED58F5D963519F2D465887E7D0F7	{"transactionId":"6196c16f-6d10-4cb6-8999-65f4946b94cb","fromAccountId":"2e9d6587-1940-4ba5-b95b-2cdd31507e30","recipientAccountId":"5b0fbd99-69c9-435b-81f5-6c931654fcee","recipientAccountName":"Gold Coast House","recipientName":"Nikhil Naik","recipientEmail":"nikhilsurfingaus@gmail.com","amount":69,"currency":"AUD","reference":"Dog Food","status":"Completed","createdAt":"2026-07-19T10:25:25.616133Z"}	2026-07-19 20:25:25.611725+10	2026-07-20 20:25:25.611725+10
8cc1c6cc-54e9-447e-8c77-728db51f80f0	8303abc0-f937-443b-a4d4-3e60943bd482	aa9251bc-9430-4e73-b8ef-12d018ef8bc8	8EF437EDF05BEBB0EB72B2D9052B60D2A5AF060ABF3B3B11F3686D541A27F662	{"transactionId":"2639942f-252a-402d-94a3-2e8c85e75cab","fromAccountId":"5fa23e09-4221-4ccb-bbf9-ae68b73f58f1","recipientAccountId":"5b0fbd99-69c9-435b-81f5-6c931654fcee","recipientAccountName":"Gold Coast House","recipientName":"Nikhil Naik","recipientEmail":"nikhilsurfingaus@gmail.com","amount":25,"currency":"AUD","reference":"Fuel","status":"Completed","createdAt":"2026-07-19T10:35:02.413946Z"}	2026-07-19 20:35:02.411066+10	2026-07-20 20:35:02.411066+10
c21f0679-0b0e-430a-a74a-d8408d929c0b	79483998-193a-4b74-b8a6-a3c98e3131e2	228864c1-557e-4f86-98e6-5bf8f7c2990f	2FABEA85DF4A0AC4F3F0A40D58A183BB71886B038BDCEA61868AEA939D987A4E	{"transactionId":"545cc0f8-c773-45c3-8aca-4ca71307ea80","fromAccountId":"5b0fbd99-69c9-435b-81f5-6c931654fcee","recipientAccountId":"2e9d6587-1940-4ba5-b95b-2cdd31507e30","recipientAccountName":"Bills","recipientName":"Mirabel Suttcliffe","recipientEmail":"cobrastormgaming@gmail.com","amount":1.67,"currency":"AUD","reference":"Tax","status":"Completed","createdAt":"2026-07-23T05:28:22.309053Z"}	2026-07-23 15:28:22.177055+10	2026-07-24 15:28:22.177055+10
9d96c177-9a6d-4e71-b9d4-b78a7157f074	79483998-193a-4b74-b8a6-a3c98e3131e2	85520c3c-4eff-4448-aaaf-09911f842bd2	2FABEA85DF4A0AC4F3F0A40D58A183BB71886B038BDCEA61868AEA939D987A4E	{"transactionId":"7ab8773e-bd3b-4375-a38d-dd959379a91e","fromAccountId":"5b0fbd99-69c9-435b-81f5-6c931654fcee","recipientAccountId":"2e9d6587-1940-4ba5-b95b-2cdd31507e30","recipientAccountName":"Bills","recipientName":"Mirabel Suttcliffe","recipientEmail":"cobrastormgaming@gmail.com","amount":1.67,"currency":"AUD","reference":"Tax","status":"Completed","createdAt":"2026-07-23T05:28:36.090809Z"}	2026-07-23 15:28:36.083402+10	2026-07-24 15:28:36.083402+10
5b29251b-d53a-4736-8f19-0dbe22d385b9	79483998-193a-4b74-b8a6-a3c98e3131e2	2d10863f-9e6b-471b-af04-d1d1d4b1d7b9	2FABEA85DF4A0AC4F3F0A40D58A183BB71886B038BDCEA61868AEA939D987A4E	{"transactionId":"983d9129-9ebc-4b6d-9d6d-7c29326780d9","fromAccountId":"5b0fbd99-69c9-435b-81f5-6c931654fcee","recipientAccountId":"2e9d6587-1940-4ba5-b95b-2cdd31507e30","recipientAccountName":"Bills","recipientName":"Mirabel Suttcliffe","recipientEmail":"cobrastormgaming@gmail.com","amount":1.67,"currency":"AUD","reference":"Tax","status":"Completed","createdAt":"2026-07-23T05:34:02.793581Z"}	2026-07-23 15:34:02.787729+10	2026-07-24 15:34:02.787729+10
a2edb333-02ae-445d-9d5d-6f6471a97e09	79483998-193a-4b74-b8a6-a3c98e3131e2	3efb1ff0-2336-4487-86fa-201a47b5f3b8	2FABEA85DF4A0AC4F3F0A40D58A183BB71886B038BDCEA61868AEA939D987A4E	{"transactionId":"1966c3df-8a4b-47c5-9b9d-984ccad83529","fromAccountId":"5b0fbd99-69c9-435b-81f5-6c931654fcee","recipientAccountId":"2e9d6587-1940-4ba5-b95b-2cdd31507e30","recipientAccountName":"Bills","recipientName":"Mirabel Suttcliffe","recipientEmail":"cobrastormgaming@gmail.com","amount":1.67,"currency":"AUD","reference":"Tax","status":"Completed","createdAt":"2026-07-23T05:34:03.951375Z"}	2026-07-23 15:34:03.947423+10	2026-07-24 15:34:03.947423+10
1897090a-6c6a-438c-b70c-09cca22a8f67	79483998-193a-4b74-b8a6-a3c98e3131e2	27b2f6c3-c304-43cf-8b5b-e5c3770acf09	2FABEA85DF4A0AC4F3F0A40D58A183BB71886B038BDCEA61868AEA939D987A4E	{"transactionId":"43333da0-dfc2-47e0-90d2-61a1ebc49972","fromAccountId":"5b0fbd99-69c9-435b-81f5-6c931654fcee","recipientAccountId":"2e9d6587-1940-4ba5-b95b-2cdd31507e30","recipientAccountName":"Bills","recipientName":"Mirabel Suttcliffe","recipientEmail":"cobrastormgaming@gmail.com","amount":1.67,"currency":"AUD","reference":"Tax","status":"Completed","createdAt":"2026-07-23T05:34:22.574846Z"}	2026-07-23 15:34:22.352868+10	2026-07-24 15:34:22.352868+10
49197d9e-4835-49dc-b138-d6684c5c2d11	79483998-193a-4b74-b8a6-a3c98e3131e2	c83b7ecb-f290-4613-9f15-5784a14bf936	00410E887A921EE4223365F1D8008731F4F4DC76AD04A3F8C212E4993D46FBA7	{"transactionId":"36d8f0b7-d385-4585-b112-e79451ec1b96","fromAccountId":"5b0fbd99-69c9-435b-81f5-6c931654fcee","recipientAccountId":"2e9d6587-1940-4ba5-b95b-2cdd31507e30","recipientAccountName":"Bills","recipientName":"Mirabel Suttcliffe","recipientEmail":"cobrastormgaming@gmail.com","amount":3.32,"currency":"AUD","reference":"ello","status":"Completed","createdAt":"2026-07-23T05:39:24.078121Z"}	2026-07-23 15:39:24.07268+10	2026-07-24 15:39:24.07268+10
\.


--
-- Data for Name: LedgerEntries; Type: TABLE DATA; Schema: public; Owner: nikhil
--

COPY public."LedgerEntries" ("Id", "TransactionId", "AccountId", "EntryType", "Amount", "CreatedAt") FROM stdin;
b7d755ea-f0ac-4603-a164-fcf4a19df0e8	f54356e9-9a54-4a25-a728-d143e7e037b4	cc0afab3-d4c6-4d85-abbd-5033ce2c24d1	Credit	500.00	2026-07-03 02:29:16.209261+10
da0bc4e3-da49-4563-a379-f147d01c72b0	f54356e9-9a54-4a25-a728-d143e7e037b4	2e9d6587-1940-4ba5-b95b-2cdd31507e30	Debit	500.00	2026-07-03 02:29:16.209261+10
5bfdeb68-dabb-4752-af0b-82ef710dc75b	c1228b31-68f3-48df-9d75-19cecf06bd2d	c745c7d8-e1a3-4f27-90cd-6413190b9cac	Debit	1000.00	2026-07-03 02:31:38.316833+10
d9b7024f-ab42-4f30-b5ed-eabee6b039d6	c1228b31-68f3-48df-9d75-19cecf06bd2d	2e9d6587-1940-4ba5-b95b-2cdd31507e30	Credit	1000.00	2026-07-03 02:31:38.316833+10
0af79f6b-91de-4342-9943-808aa20290d0	e8903e82-cd67-4c5b-a66d-598cc261caaa	5b0fbd99-69c9-435b-81f5-6c931654fcee	Debit	22000.00	2026-07-03 21:13:07.094913+10
7ec9d032-e1bb-43c4-a488-d908afc37bc2	e8903e82-cd67-4c5b-a66d-598cc261caaa	c745c7d8-e1a3-4f27-90cd-6413190b9cac	Credit	22000.00	2026-07-03 21:13:07.094913+10
3f155f51-37ec-4f68-9fef-2fdfb6ceb4be	6f9de903-e67a-492c-baa4-fe141f8c6650	5b0fbd99-69c9-435b-81f5-6c931654fcee	Debit	38000.00	2026-07-05 03:26:23.803102+10
46708e32-462b-4c7b-aae7-e71217e28424	6f9de903-e67a-492c-baa4-fe141f8c6650	c745c7d8-e1a3-4f27-90cd-6413190b9cac	Credit	38000.00	2026-07-05 03:26:23.803102+10
1245b278-9d8c-4545-9172-d50f712cfaab	be6eaccf-08c4-4ede-b4ca-7327db112853	4136fc78-722d-409e-82f6-1c8d8dc57a41	Credit	21000.00	2026-07-05 20:30:10.683237+10
9d2b4d39-875d-4f1b-9810-f31045a632f7	be6eaccf-08c4-4ede-b4ca-7327db112853	c745c7d8-e1a3-4f27-90cd-6413190b9cac	Debit	21000.00	2026-07-05 20:30:10.683237+10
444f451a-a944-43b6-a68e-242a027d3095	13c87ebc-0f6d-4cb3-b21f-48e0b0f465e1	c745c7d8-e1a3-4f27-90cd-6413190b9cac	Debit	9000.00	2026-07-05 20:33:05.152337+10
d0f9714a-a228-40ec-92b9-f49b50840317	13c87ebc-0f6d-4cb3-b21f-48e0b0f465e1	4136fc78-722d-409e-82f6-1c8d8dc57a41	Credit	9000.00	2026-07-05 20:33:05.152337+10
2d57e355-6270-439c-b4a0-6bacfcecb6c8	617860ea-37bf-402f-9d2f-317f2aa550a5	4136fc78-722d-409e-82f6-1c8d8dc57a41	Debit	4000.00	2026-07-05 20:36:27.423919+10
413972bd-aac3-4b67-bb85-3f03dec39dd3	617860ea-37bf-402f-9d2f-317f2aa550a5	c745c7d8-e1a3-4f27-90cd-6413190b9cac	Credit	4000.00	2026-07-05 20:36:27.423919+10
0ab90a5a-1937-4b58-bbe9-9ffb3d6ecc26	4a85afcb-c059-46ec-9fe0-e21d6cdd8657	38b99e28-08f2-4038-9a42-eb4987d824a0	Credit	2000.00	2026-07-05 21:52:44.012231+10
9836d6eb-84ca-422a-9293-eeacbc4eba66	4a85afcb-c059-46ec-9fe0-e21d6cdd8657	4136fc78-722d-409e-82f6-1c8d8dc57a41	Debit	2000.00	2026-07-05 21:52:44.012231+10
799d19af-5d22-4e07-8003-24578ead0baa	75ea72d5-118f-4862-bcd8-1317f0e42994	5b0fbd99-69c9-435b-81f5-6c931654fcee	Debit	500000.00	2026-07-05 21:58:26.968129+10
98d3f3ba-b6cf-487a-8f3f-d1ccb21b86d4	75ea72d5-118f-4862-bcd8-1317f0e42994	b45d4b83-54df-45ea-867c-86c34b842421	Credit	500000.00	2026-07-05 21:58:26.968129+10
a70d92d0-9051-45f9-bed7-9ed2af674182	1c88f6b4-84df-4319-a2d1-c50fe9df3b2c	7f319585-0d71-49e2-bbbb-282e9c3c971c	Credit	2000.00	2026-07-05 21:59:12.637172+10
d9dfb682-7f05-4262-808a-a752785731ce	1c88f6b4-84df-4319-a2d1-c50fe9df3b2c	c745c7d8-e1a3-4f27-90cd-6413190b9cac	Debit	2000.00	2026-07-05 21:59:12.637172+10
5ad7ed18-f8c2-40a2-9c0c-8224ac741dd1	8c56930b-d719-4c99-ace9-e555f92c2383	b45d4b83-54df-45ea-867c-86c34b842421	Debit	100000.00	2026-07-09 02:07:07.659668+10
c4f08b8b-3cf5-44ed-a8a1-1122b5cdc94e	8c56930b-d719-4c99-ace9-e555f92c2383	5b0fbd99-69c9-435b-81f5-6c931654fcee	Credit	100000.00	2026-07-09 02:07:07.659668+10
836ba825-40b1-4d45-8acf-22912a2c9dc9	ecdfd77f-7662-4ba0-bbfc-3559b9e048a6	2e9d6587-1940-4ba5-b95b-2cdd31507e30	Credit	69.00	2026-07-16 23:09:04.404661+10
f15d7fcd-cd74-48f9-a496-e76a6f739a77	ecdfd77f-7662-4ba0-bbfc-3559b9e048a6	b45d4b83-54df-45ea-867c-86c34b842421	Debit	69.00	2026-07-16 23:09:04.404661+10
3dbc08a1-3a52-4c2e-89e8-5bc526bae84c	378092e8-194f-430e-9900-a53e8961e92d	2e9d6587-1940-4ba5-b95b-2cdd31507e30	Credit	3000.00	2026-07-19 20:15:51.565197+10
fa080d92-0e5a-474b-963c-7347bac5d6a9	378092e8-194f-430e-9900-a53e8961e92d	c745c7d8-e1a3-4f27-90cd-6413190b9cac	Debit	3000.00	2026-07-19 20:15:51.565197+10
61d212ba-b2fd-4b0b-ba34-0eb6b2c8e0d5	6196c16f-6d10-4cb6-8999-65f4946b94cb	5b0fbd99-69c9-435b-81f5-6c931654fcee	Credit	69.00	2026-07-19 20:25:25.616133+10
b59299e2-4a50-4932-b35a-65f4758df393	6196c16f-6d10-4cb6-8999-65f4946b94cb	2e9d6587-1940-4ba5-b95b-2cdd31507e30	Debit	69.00	2026-07-19 20:25:25.616133+10
1ac71bc0-fae4-4f76-bd51-23c8ac01efeb	2639942f-252a-402d-94a3-2e8c85e75cab	5fa23e09-4221-4ccb-bbf9-ae68b73f58f1	Debit	25.00	2026-07-19 20:35:02.413946+10
ebd2b5af-165f-40ce-ba4c-71040698b90f	2639942f-252a-402d-94a3-2e8c85e75cab	5b0fbd99-69c9-435b-81f5-6c931654fcee	Credit	25.00	2026-07-19 20:35:02.413946+10
a1e13d16-ace4-4c8f-99f5-998543c04bb5	cbe99bf0-2efb-4753-bce2-93aa0aaf035a	4136fc78-722d-409e-82f6-1c8d8dc57a41	Debit	8000.00	2026-07-19 20:37:13.535918+10
ae4d33da-3a00-478e-b36b-9d428709bae3	cbe99bf0-2efb-4753-bce2-93aa0aaf035a	38b99e28-08f2-4038-9a42-eb4987d824a0	Credit	8000.00	2026-07-19 20:37:13.535918+10
22ef9746-facb-4843-a322-df447b7112e0	7aa0deec-a847-4fa1-81b9-44434a2c1432	5b0fbd99-69c9-435b-81f5-6c931654fcee	Credit	67.67	2026-07-22 20:31:32.244383+10
3d714edf-4f9a-40d3-8fa4-7f97c55a28e0	7aa0deec-a847-4fa1-81b9-44434a2c1432	22222222-2222-2222-2222-222222222222	Debit	67.67	2026-07-22 20:31:32.244383+10
58b3b0c7-2cf1-495e-913d-8c96dff3e305	70d77cee-b4d7-4f7e-8788-3978af9cc811	2e9d6587-1940-4ba5-b95b-2cdd31507e30	Credit	123.00	2026-07-22 20:33:06.540871+10
fdbe683e-9b73-4ac9-8567-498a40cc6383	70d77cee-b4d7-4f7e-8788-3978af9cc811	22222222-2222-2222-2222-222222222222	Debit	123.00	2026-07-22 20:33:06.540871+10
1f516242-631b-404e-8ee0-364284fe1b98	6c8c34ee-d053-4cd4-8e30-c7de8891e994	2e9d6587-1940-4ba5-b95b-2cdd31507e30	Credit	2500.00	2026-07-22 20:33:18.274912+10
8d5fd8ce-ade7-4a79-81d2-a6f3d469637f	6c8c34ee-d053-4cd4-8e30-c7de8891e994	22222222-2222-2222-2222-222222222222	Debit	2500.00	2026-07-22 20:33:18.274912+10
a61bd837-5267-49e9-b561-55c3cb9350ec	2c4e93df-676c-420f-915a-abcc0521201f	2e9d6587-1940-4ba5-b95b-2cdd31507e30	Credit	2500.00	2026-07-22 20:35:13.70129+10
b50cd086-f34f-436e-8b78-c11cdfa57e13	2c4e93df-676c-420f-915a-abcc0521201f	22222222-2222-2222-2222-222222222222	Debit	2500.00	2026-07-22 20:35:13.70129+10
0f2d4fb5-4510-4e34-8393-71e0dd3e2bf0	7e7c4d9a-3745-4b4b-a55b-ed8e6cfccde6	22222222-2222-2222-2222-222222222222	Debit	2500.00	2026-07-22 20:35:25.350543+10
7fa2b4ca-bafe-4fd1-954a-f951363e73d7	7e7c4d9a-3745-4b4b-a55b-ed8e6cfccde6	5b0fbd99-69c9-435b-81f5-6c931654fcee	Credit	2500.00	2026-07-22 20:35:25.350543+10
6ec936df-fb73-410b-9a90-b4ec4e653950	56facccf-58dd-4c0c-b84a-b987e9226ead	22222222-2222-2222-2222-222222222222	Debit	55000.00	2026-07-22 21:10:17.476593+10
730cd7b4-14d4-4bb7-82aa-e975d83fe167	56facccf-58dd-4c0c-b84a-b987e9226ead	9086bb3b-e80b-4aa7-a788-f5f71d798657	Credit	55000.00	2026-07-22 21:10:17.476593+10
7517dbab-b6e5-4a8c-b561-bbbb4a332b4a	f2c22a71-97e0-46e2-b797-5eb7b26dca1e	9086bb3b-e80b-4aa7-a788-f5f71d798657	Credit	15000.00	2026-07-22 21:34:59.223596+10
cdcbbddb-0845-492c-a02c-b6fec6ff91ab	f2c22a71-97e0-46e2-b797-5eb7b26dca1e	22222222-2222-2222-2222-222222222222	Debit	15000.00	2026-07-22 21:34:59.223596+10
7711e91f-7dfb-406c-be49-01c5c41ad5da	f87ff30c-0bc2-4f2b-8994-2656a069caf2	9086bb3b-e80b-4aa7-a788-f5f71d798657	Debit	15000.00	2026-07-22 21:41:20.641331+10
a462d3e8-c8ef-4b0f-9c26-e3526dc55d5c	f87ff30c-0bc2-4f2b-8994-2656a069caf2	38b99e28-08f2-4038-9a42-eb4987d824a0	Credit	15000.00	2026-07-22 21:41:20.641331+10
4f5dab81-32b3-4ea2-9c96-9958f1e1386e	c90ed5c7-4375-48e4-b246-a07872f704ca	22222222-2222-2222-2222-222222222222	Credit	12990.00	2026-07-22 21:45:17.641752+10
a15c1407-99d5-475c-b102-ca3b211539f7	c90ed5c7-4375-48e4-b246-a07872f704ca	9086bb3b-e80b-4aa7-a788-f5f71d798657	Debit	12990.00	2026-07-22 21:45:17.641752+10
031598b5-98eb-4ea2-a0f4-4f51ad10f273	545cc0f8-c773-45c3-8aca-4ca71307ea80	5b0fbd99-69c9-435b-81f5-6c931654fcee	Debit	1.67	2026-07-23 15:28:22.309053+10
2fb60610-f2b6-4dd9-a25b-94c5aa6857ed	545cc0f8-c773-45c3-8aca-4ca71307ea80	2e9d6587-1940-4ba5-b95b-2cdd31507e30	Credit	1.67	2026-07-23 15:28:22.309053+10
163c09e6-34ef-4d72-9792-454b9be9f8be	7ab8773e-bd3b-4375-a38d-dd959379a91e	5b0fbd99-69c9-435b-81f5-6c931654fcee	Debit	1.67	2026-07-23 15:28:36.090809+10
c98e2c0e-255a-46d7-bb16-50d99bb08d3a	7ab8773e-bd3b-4375-a38d-dd959379a91e	2e9d6587-1940-4ba5-b95b-2cdd31507e30	Credit	1.67	2026-07-23 15:28:36.090809+10
0f585fba-1c52-4e50-8d18-1bf7f6048fd8	983d9129-9ebc-4b6d-9d6d-7c29326780d9	2e9d6587-1940-4ba5-b95b-2cdd31507e30	Credit	1.67	2026-07-23 15:34:02.793581+10
d8d8dd12-3e1e-44ac-a302-c424cb642730	983d9129-9ebc-4b6d-9d6d-7c29326780d9	5b0fbd99-69c9-435b-81f5-6c931654fcee	Debit	1.67	2026-07-23 15:34:02.793581+10
23a2516e-17d0-49b2-929f-d3f3edcccfee	1966c3df-8a4b-47c5-9b9d-984ccad83529	5b0fbd99-69c9-435b-81f5-6c931654fcee	Debit	1.67	2026-07-23 15:34:03.951375+10
811607bb-69f4-4472-9254-2a22f943f1ef	1966c3df-8a4b-47c5-9b9d-984ccad83529	2e9d6587-1940-4ba5-b95b-2cdd31507e30	Credit	1.67	2026-07-23 15:34:03.951375+10
25fab9fd-ba46-4a96-bbc5-ac5aa8f0e9ec	43333da0-dfc2-47e0-90d2-61a1ebc49972	5b0fbd99-69c9-435b-81f5-6c931654fcee	Debit	1.67	2026-07-23 15:34:22.574846+10
425a8743-e891-4fad-b803-701e36d764ad	43333da0-dfc2-47e0-90d2-61a1ebc49972	2e9d6587-1940-4ba5-b95b-2cdd31507e30	Credit	1.67	2026-07-23 15:34:22.574846+10
a544aee4-15ec-4cdb-b2bc-b243e373ac44	36d8f0b7-d385-4585-b112-e79451ec1b96	2e9d6587-1940-4ba5-b95b-2cdd31507e30	Credit	3.32	2026-07-23 15:39:24.078121+10
bce978a1-2143-4b4c-99dc-ede87bcdeb1f	36d8f0b7-d385-4585-b112-e79451ec1b96	5b0fbd99-69c9-435b-81f5-6c931654fcee	Debit	3.32	2026-07-23 15:39:24.078121+10
\.


--
-- Data for Name: LoanRepayments; Type: TABLE DATA; Schema: public; Owner: nikhil
--

COPY public."LoanRepayments" ("Id", "LoanId", "TransactionId", "Amount", "PrincipalPortion", "InterestPortion", "CreatedAt") FROM stdin;
959ee00d-d4a3-49fa-814d-87b4abc632c5	1d90e4ff-7c68-4f19-a574-6ccde096ecc9	c90ed5c7-4375-48e4-b246-a07872f704ca	12990.00	12990.00	0.00	2026-07-22 21:45:17.641752+10
\.


--
-- Data for Name: Loans; Type: TABLE DATA; Schema: public; Owner: nikhil
--

COPY public."Loans" ("Id", "UserId", "BranchId", "FundedAccountId", "DisbursementTransactionId", "Principal", "OutstandingPrincipal", "InterestRatePercent", "MonthlyPayment", "Currency", "TermMonths", "StartDate", "NextDueDate", "MaturityDate", "Status", "Purpose", "RejectionReason", "CreatedAt", "UpdatedAt") FROM stdin;
fd055eec-34b9-4088-baf0-7c21395c2243	79483998-193a-4b74-b8a6-a3c98e3131e2	33333333-3333-3333-3333-333333333333	4136fc78-722d-409e-82f6-1c8d8dc57a41	\N	12000.00	12000.00	5.0000	1027.29	AUD	12	\N	\N	\N	Cancelled	J120 Prado + Mods	\N	2026-07-22 21:10:41.659089+10	2026-07-22 21:10:59.765323+10
9a121d74-abd1-4da0-a240-0d536fb5880e	79483998-193a-4b74-b8a6-a3c98e3131e2	33333333-3333-3333-3333-333333333333	4136fc78-722d-409e-82f6-1c8d8dc57a41	\N	25000.00	25000.00	5.0000	2140.19	AUD	12	\N	\N	\N	Rejected	crypto	No Crypto	2026-07-22 21:29:27.608156+10	2026-07-22 21:34:53.345242+10
1d90e4ff-7c68-4f19-a574-6ccde096ecc9	79483998-193a-4b74-b8a6-a3c98e3131e2	33333333-3333-3333-3333-333333333333	9086bb3b-e80b-4aa7-a788-f5f71d798657	56facccf-58dd-4c0c-b84a-b987e9226ead	15000.00	2010.00	5.0000	1284.11	AUD	12	2026-07-22	2026-09-22	2027-07-22	Active	J120 Prado + Mods	\N	2026-07-22 21:11:15.083159+10	2026-07-22 21:45:17.641752+10
\.


--
-- Data for Name: Notifications; Type: TABLE DATA; Schema: public; Owner: nikhil
--

COPY public."Notifications" ("Id", "UserId", "Title", "Body", "NotificationType", "IsRead", "CreatedAt", "RelatedEntityId") FROM stdin;
6334cb16-e7a8-44f1-9e98-8ecc2be2545c	79483998-193a-4b74-b8a6-a3c98e3131e2	Payment sent	3.32 AUD to Mirabel Suttcliffe	PaymentSent	f	2026-07-23 15:39:24.084663+10	36d8f0b7-d385-4585-b112-e79451ec1b96
dba3bd9e-36f3-4d28-baae-eb8111539de8	04d20afb-e7eb-488c-814e-3c865a3c38ea	Payment received	3.32 AUD from Nikhil Naik	PaymentReceived	t	2026-07-23 15:39:24.086144+10	36d8f0b7-d385-4585-b112-e79451ec1b96
\.


--
-- Data for Name: Transactions; Type: TABLE DATA; Schema: public; Owner: nikhil
--

COPY public."Transactions" ("Id", "FromAccountId", "ToAccountId", "Amount", "Description", "Status", "CreatedAt", "Category", "Merchant", "Reference") FROM stdin;
f54356e9-9a54-4a25-a728-d143e7e037b4	2e9d6587-1940-4ba5-b95b-2cdd31507e30	cc0afab3-d4c6-4d85-abbd-5033ce2c24d1	500.00	Move money to Euro Summer savings	Completed	2026-07-03 02:29:16.209261+10	Transfer	\N	\N
c1228b31-68f3-48df-9d75-19cecf06bd2d	c745c7d8-e1a3-4f27-90cd-6413190b9cac	2e9d6587-1940-4ba5-b95b-2cdd31507e30	1000.00	NVIDIA profit to Bills	Completed	2026-07-03 02:31:38.316833+10	Transfer	\N	\N
e8903e82-cd67-4c5b-a66d-598cc261caaa	5b0fbd99-69c9-435b-81f5-6c931654fcee	c745c7d8-e1a3-4f27-90cd-6413190b9cac	22000.00	Market Downturn Investment	Completed	2026-07-03 21:13:07.094913+10	Transfer	\N	\N
6f9de903-e67a-492c-baa4-fe141f8c6650	5b0fbd99-69c9-435b-81f5-6c931654fcee	c745c7d8-e1a3-4f27-90cd-6413190b9cac	38000.00	Remortage	Completed	2026-07-05 03:26:23.803102+10	Transfer	\N	\N
be6eaccf-08c4-4ede-b4ca-7327db112853	c745c7d8-e1a3-4f27-90cd-6413190b9cac	4136fc78-722d-409e-82f6-1c8d8dc57a41	21000.00	Crypto Transfer	Completed	2026-07-05 20:30:10.683237+10	Transfer	\N	\N
13c87ebc-0f6d-4cb3-b21f-48e0b0f465e1	c745c7d8-e1a3-4f27-90cd-6413190b9cac	4136fc78-722d-409e-82f6-1c8d8dc57a41	9000.00		Completed	2026-07-05 20:33:05.152337+10	Transfer	\N	\N
617860ea-37bf-402f-9d2f-317f2aa550a5	4136fc78-722d-409e-82f6-1c8d8dc57a41	c745c7d8-e1a3-4f27-90cd-6413190b9cac	4000.00		Completed	2026-07-05 20:36:27.423919+10	Transfer	\N	\N
4a85afcb-c059-46ec-9fe0-e21d6cdd8657	4136fc78-722d-409e-82f6-1c8d8dc57a41	38b99e28-08f2-4038-9a42-eb4987d824a0	2000.00	Contribution to Prado J120	Completed	2026-07-05 21:52:44.012231+10	Transfer	\N	\N
75ea72d5-118f-4862-bcd8-1317f0e42994	5b0fbd99-69c9-435b-81f5-6c931654fcee	b45d4b83-54df-45ea-867c-86c34b842421	500000.00	Contribution to Gold Coast Property	Completed	2026-07-05 21:58:26.968129+10	Transfer	\N	\N
1c88f6b4-84df-4319-a2d1-c50fe9df3b2c	c745c7d8-e1a3-4f27-90cd-6413190b9cac	7f319585-0d71-49e2-bbbb-282e9c3c971c	2000.00	Contribution to Caravan Park Holiday	Completed	2026-07-05 21:59:12.637172+10	Transfer	\N	\N
8c56930b-d719-4c99-ace9-e555f92c2383	b45d4b83-54df-45ea-867c-86c34b842421	5b0fbd99-69c9-435b-81f5-6c931654fcee	100000.00	Withdrawal from Gold Coast Property	Completed	2026-07-09 02:07:07.659668+10	Transfer	\N	\N
ecdfd77f-7662-4ba0-bbfc-3559b9e048a6	b45d4b83-54df-45ea-867c-86c34b842421	2e9d6587-1940-4ba5-b95b-2cdd31507e30	69.00	Payment to Mirabel Suttcliffe	Completed	2026-07-16 23:09:04.404661+10	Transfer	\N	Dinner xoxo
378092e8-194f-430e-9900-a53e8961e92d	c745c7d8-e1a3-4f27-90cd-6413190b9cac	2e9d6587-1940-4ba5-b95b-2cdd31507e30	3000.00	Payment to Mirabel Suttcliffe	Completed	2026-07-19 20:15:51.565197+10	Transfer	\N	Mechanic Fees
6196c16f-6d10-4cb6-8999-65f4946b94cb	2e9d6587-1940-4ba5-b95b-2cdd31507e30	5b0fbd99-69c9-435b-81f5-6c931654fcee	69.00	Payment to Nikhil Naik	Completed	2026-07-19 20:25:25.616133+10	Transfer	\N	Dog Food
2639942f-252a-402d-94a3-2e8c85e75cab	5fa23e09-4221-4ccb-bbf9-ae68b73f58f1	5b0fbd99-69c9-435b-81f5-6c931654fcee	25.00	Payment to Nikhil Naik	Completed	2026-07-19 20:35:02.413946+10	Transfer	\N	Fuel
cbe99bf0-2efb-4753-bce2-93aa0aaf035a	4136fc78-722d-409e-82f6-1c8d8dc57a41	38b99e28-08f2-4038-9a42-eb4987d824a0	8000.00	Sold Subaru	Completed	2026-07-19 20:37:13.535918+10	Transfer	\N	\N
7aa0deec-a847-4fa1-81b9-44434a2c1432	22222222-2222-2222-2222-222222222222	5b0fbd99-69c9-435b-81f5-6c931654fcee	67.67	67	Completed	2026-07-22 20:31:32.244383+10	Income	\N	BRANCH-20260722103132
70d77cee-b4d7-4f7e-8788-3978af9cc811	22222222-2222-2222-2222-222222222222	2e9d6587-1940-4ba5-b95b-2cdd31507e30	123.00	123	Completed	2026-07-22 20:33:06.540871+10	Income	\N	BRANCH-20260722103306
6c8c34ee-d053-4cd4-8e30-c7de8891e994	22222222-2222-2222-2222-222222222222	2e9d6587-1940-4ba5-b95b-2cdd31507e30	2500.00	Branch top-up from Current HQ	Completed	2026-07-22 20:33:18.274912+10	Income	\N	BRANCH-20260722103318
2c4e93df-676c-420f-915a-abcc0521201f	22222222-2222-2222-2222-222222222222	2e9d6587-1940-4ba5-b95b-2cdd31507e30	2500.00	Branch top-up from Current HQ	Completed	2026-07-22 20:35:13.70129+10	Income	\N	BRANCH-20260722103513
7e7c4d9a-3745-4b4b-a55b-ed8e6cfccde6	22222222-2222-2222-2222-222222222222	5b0fbd99-69c9-435b-81f5-6c931654fcee	2500.00	Branch top-up from Current HQ	Completed	2026-07-22 20:35:25.350543+10	Income	\N	BRANCH-20260722103525
56facccf-58dd-4c0c-b84a-b987e9226ead	22222222-2222-2222-2222-222222222222	9086bb3b-e80b-4aa7-a788-f5f71d798657	55000.00	Branch top-up from Current HQ	Completed	2026-07-22 21:10:17.476593+10	Income	\N	BRANCH-20260722111017
f2c22a71-97e0-46e2-b797-5eb7b26dca1e	22222222-2222-2222-2222-222222222222	9086bb3b-e80b-4aa7-a788-f5f71d798657	15000.00	Loan disbursement from Current HQ	Completed	2026-07-22 21:34:59.223596+10	Income	\N	BRANCH-20260722113459
f87ff30c-0bc2-4f2b-8994-2656a069caf2	9086bb3b-e80b-4aa7-a788-f5f71d798657	38b99e28-08f2-4038-9a42-eb4987d824a0	15000.00	loan	Completed	2026-07-22 21:41:20.641331+10	Transfer	\N	\N
c90ed5c7-4375-48e4-b246-a07872f704ca	9086bb3b-e80b-4aa7-a788-f5f71d798657	22222222-2222-2222-2222-222222222222	12990.00	Loan repayment to Current HQ	Completed	2026-07-22 21:45:17.641752+10	Transfer	\N	LOAN-REPAY-20260722114517
545cc0f8-c773-45c3-8aca-4ca71307ea80	5b0fbd99-69c9-435b-81f5-6c931654fcee	2e9d6587-1940-4ba5-b95b-2cdd31507e30	1.67	Payment to Mirabel Suttcliffe	Completed	2026-07-23 15:28:22.309053+10	Transfer	\N	Tax
7ab8773e-bd3b-4375-a38d-dd959379a91e	5b0fbd99-69c9-435b-81f5-6c931654fcee	2e9d6587-1940-4ba5-b95b-2cdd31507e30	1.67	Payment to Mirabel Suttcliffe	Completed	2026-07-23 15:28:36.090809+10	Transfer	\N	Tax
983d9129-9ebc-4b6d-9d6d-7c29326780d9	5b0fbd99-69c9-435b-81f5-6c931654fcee	2e9d6587-1940-4ba5-b95b-2cdd31507e30	1.67	Payment to Mirabel Suttcliffe	Completed	2026-07-23 15:34:02.793581+10	Transfer	\N	Tax
1966c3df-8a4b-47c5-9b9d-984ccad83529	5b0fbd99-69c9-435b-81f5-6c931654fcee	2e9d6587-1940-4ba5-b95b-2cdd31507e30	1.67	Payment to Mirabel Suttcliffe	Completed	2026-07-23 15:34:03.951375+10	Transfer	\N	Tax
43333da0-dfc2-47e0-90d2-61a1ebc49972	5b0fbd99-69c9-435b-81f5-6c931654fcee	2e9d6587-1940-4ba5-b95b-2cdd31507e30	1.67	Payment to Mirabel Suttcliffe	Completed	2026-07-23 15:34:22.574846+10	Transfer	\N	Tax
36d8f0b7-d385-4585-b112-e79451ec1b96	5b0fbd99-69c9-435b-81f5-6c931654fcee	2e9d6587-1940-4ba5-b95b-2cdd31507e30	3.32	Payment to Mirabel Suttcliffe	Completed	2026-07-23 15:39:24.078121+10	Transfer	\N	ello
\.


--
-- Data for Name: Users; Type: TABLE DATA; Schema: public; Owner: nikhil
--

COPY public."Users" ("Id", "FirstName", "LastName", "Email", "CreatedAt", "UpdatedAt", "PasswordHash", "Role", "Locale", "PreferredCurrency", "ThemePreference", "Timezone") FROM stdin;
8303abc0-f937-443b-a4d4-3e60943bd482	Gordan	Ramsey	aussiepilotlife@gmail.com	2026-07-03 21:04:29.510103+10	2026-07-03 21:04:29.510103+10	AQAAAAIAAYagAAAAEG98Z9c+JjXKVhm4RwjWuBJauaOT7Bq7VP9deFu3yPxON7NlFgoU2j6RwvaaMLKstg==	User	en-AU	AUD	System	Australia/Sydney
04d20afb-e7eb-488c-814e-3c865a3c38ea	Mirabel	Suttcliffe	cobrastormgaming@gmail.com	2026-07-01 21:36:53.735352+10	2026-07-01 21:36:53.735352+10	AQAAAAIAAYagAAAAEG98Z9c+JjXKVhm4RwjWuBJauaOT7Bq7VP9deFu3yPxON7NlFgoU2j6RwvaaMLKstg==	User	en-AU	AUD	System	Australia/Sydney
79483998-193a-4b74-b8a6-a3c98e3131e2	Nikhil	Naik	nikhilsurfingaus@gmail.com	2026-07-01 21:28:06.736712+10	2026-07-22 19:25:08.253326+10	AQAAAAIAAYagAAAAEG98Z9c+JjXKVhm4RwjWuBJauaOT7Bq7VP9deFu3yPxON7NlFgoU2j6RwvaaMLKstg==	User	en-AU	AUD	Light	Australia/Sydney
11111111-1111-1111-1111-111111111111	Current	Branch	branch-system@current.internal	2026-07-22 19:40:51+10	2026-07-22 19:40:51+10	SYSTEM_NO_LOGIN	Admin	en-AU	AUD	System	Australia/Sydney
4212405e-b78d-415f-81e8-5a6da1890ae9	Admin	User	admin@current.dev	2026-07-22 20:25:00.365314+10	2026-07-22 20:35:19.316559+10	AQAAAAIAAYagAAAAEJmUBs6vBgqeVGyd4jDjbj4J2egPJ2zBfzqUlS1FvAt91JUc6yy1dg/bD4VY0/ACsw==	Admin	en-AU	AUD	Light	Australia/Sydney
\.


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: nikhil
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20260701110859_InitialCreate	10.0.9
20260702113253_AddTransactionsAndLedgerEntries	10.0.9
20260703085538_AddUserAuthFields	10.0.9
20260703085654_FixUserRoleDefault	10.0.9
20260705104949_AddGoalsAndGoalContributions	10.0.9
20260705112353_AddGoalIconKey	10.0.9
20260705161506_AddTransactionAnalyticsFields	10.0.9
20260705161546_BackfillTransactionCategoryTransfer	10.0.9
20260712103000_AddIdempotencyKeys	10.0.9
20260719101500_AddContacts	10.0.9
20260722100000_AddUserPreferences	10.0.9
20260722094051_AddBranches	10.0.9
20260722104605_AddLoans	10.0.9
20260723150300_AddNotifications	10.0.9
20260723154000_AddNotificationRelatedEntityId	10.0.9
20260723154500_BackfillNotificationRelatedEntityId	10.0.9
\.


--
-- Name: Accounts PK_Accounts; Type: CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."Accounts"
    ADD CONSTRAINT "PK_Accounts" PRIMARY KEY ("Id");


--
-- Name: Branches PK_Branches; Type: CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."Branches"
    ADD CONSTRAINT "PK_Branches" PRIMARY KEY ("Id");


--
-- Name: Contacts PK_Contacts; Type: CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."Contacts"
    ADD CONSTRAINT "PK_Contacts" PRIMARY KEY ("Id");


--
-- Name: GoalContributions PK_GoalContributions; Type: CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."GoalContributions"
    ADD CONSTRAINT "PK_GoalContributions" PRIMARY KEY ("Id");


--
-- Name: Goals PK_Goals; Type: CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."Goals"
    ADD CONSTRAINT "PK_Goals" PRIMARY KEY ("Id");


--
-- Name: IdempotencyKeys PK_IdempotencyKeys; Type: CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."IdempotencyKeys"
    ADD CONSTRAINT "PK_IdempotencyKeys" PRIMARY KEY ("Id");


--
-- Name: LedgerEntries PK_LedgerEntries; Type: CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."LedgerEntries"
    ADD CONSTRAINT "PK_LedgerEntries" PRIMARY KEY ("Id");


--
-- Name: LoanRepayments PK_LoanRepayments; Type: CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."LoanRepayments"
    ADD CONSTRAINT "PK_LoanRepayments" PRIMARY KEY ("Id");


--
-- Name: Loans PK_Loans; Type: CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."Loans"
    ADD CONSTRAINT "PK_Loans" PRIMARY KEY ("Id");


--
-- Name: Notifications PK_Notifications; Type: CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "PK_Notifications" PRIMARY KEY ("Id");


--
-- Name: Transactions PK_Transactions; Type: CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."Transactions"
    ADD CONSTRAINT "PK_Transactions" PRIMARY KEY ("Id");


--
-- Name: Users PK_Users; Type: CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."Users"
    ADD CONSTRAINT "PK_Users" PRIMARY KEY ("Id");


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: IX_Accounts_UserId; Type: INDEX; Schema: public; Owner: nikhil
--

CREATE INDEX "IX_Accounts_UserId" ON public."Accounts" USING btree ("UserId");


--
-- Name: IX_Branches_Code; Type: INDEX; Schema: public; Owner: nikhil
--

CREATE UNIQUE INDEX "IX_Branches_Code" ON public."Branches" USING btree ("Code");


--
-- Name: IX_Branches_TreasuryAccountId; Type: INDEX; Schema: public; Owner: nikhil
--

CREATE INDEX "IX_Branches_TreasuryAccountId" ON public."Branches" USING btree ("TreasuryAccountId");


--
-- Name: IX_Contacts_UserId_Email; Type: INDEX; Schema: public; Owner: nikhil
--

CREATE UNIQUE INDEX "IX_Contacts_UserId_Email" ON public."Contacts" USING btree ("UserId", "Email");


--
-- Name: IX_GoalContributions_GoalId; Type: INDEX; Schema: public; Owner: nikhil
--

CREATE INDEX "IX_GoalContributions_GoalId" ON public."GoalContributions" USING btree ("GoalId");


--
-- Name: IX_GoalContributions_TransactionId; Type: INDEX; Schema: public; Owner: nikhil
--

CREATE INDEX "IX_GoalContributions_TransactionId" ON public."GoalContributions" USING btree ("TransactionId");


--
-- Name: IX_Goals_GoalAccountId; Type: INDEX; Schema: public; Owner: nikhil
--

CREATE INDEX "IX_Goals_GoalAccountId" ON public."Goals" USING btree ("GoalAccountId");


--
-- Name: IX_Goals_SourceAccountId; Type: INDEX; Schema: public; Owner: nikhil
--

CREATE INDEX "IX_Goals_SourceAccountId" ON public."Goals" USING btree ("SourceAccountId");


--
-- Name: IX_Goals_UserId; Type: INDEX; Schema: public; Owner: nikhil
--

CREATE INDEX "IX_Goals_UserId" ON public."Goals" USING btree ("UserId");


--
-- Name: IX_IdempotencyKeys_UserId_Key; Type: INDEX; Schema: public; Owner: nikhil
--

CREATE UNIQUE INDEX "IX_IdempotencyKeys_UserId_Key" ON public."IdempotencyKeys" USING btree ("UserId", "Key");


--
-- Name: IX_LedgerEntries_AccountId; Type: INDEX; Schema: public; Owner: nikhil
--

CREATE INDEX "IX_LedgerEntries_AccountId" ON public."LedgerEntries" USING btree ("AccountId");


--
-- Name: IX_LedgerEntries_TransactionId; Type: INDEX; Schema: public; Owner: nikhil
--

CREATE INDEX "IX_LedgerEntries_TransactionId" ON public."LedgerEntries" USING btree ("TransactionId");


--
-- Name: IX_LoanRepayments_LoanId; Type: INDEX; Schema: public; Owner: nikhil
--

CREATE INDEX "IX_LoanRepayments_LoanId" ON public."LoanRepayments" USING btree ("LoanId");


--
-- Name: IX_LoanRepayments_TransactionId; Type: INDEX; Schema: public; Owner: nikhil
--

CREATE INDEX "IX_LoanRepayments_TransactionId" ON public."LoanRepayments" USING btree ("TransactionId");


--
-- Name: IX_Loans_BranchId; Type: INDEX; Schema: public; Owner: nikhil
--

CREATE INDEX "IX_Loans_BranchId" ON public."Loans" USING btree ("BranchId");


--
-- Name: IX_Loans_DisbursementTransactionId; Type: INDEX; Schema: public; Owner: nikhil
--

CREATE INDEX "IX_Loans_DisbursementTransactionId" ON public."Loans" USING btree ("DisbursementTransactionId");


--
-- Name: IX_Loans_FundedAccountId; Type: INDEX; Schema: public; Owner: nikhil
--

CREATE INDEX "IX_Loans_FundedAccountId" ON public."Loans" USING btree ("FundedAccountId");


--
-- Name: IX_Loans_UserId; Type: INDEX; Schema: public; Owner: nikhil
--

CREATE INDEX "IX_Loans_UserId" ON public."Loans" USING btree ("UserId");


--
-- Name: IX_Notifications_UserId_CreatedAt; Type: INDEX; Schema: public; Owner: nikhil
--

CREATE INDEX "IX_Notifications_UserId_CreatedAt" ON public."Notifications" USING btree ("UserId", "CreatedAt");


--
-- Name: IX_Transactions_FromAccountId; Type: INDEX; Schema: public; Owner: nikhil
--

CREATE INDEX "IX_Transactions_FromAccountId" ON public."Transactions" USING btree ("FromAccountId");


--
-- Name: IX_Transactions_ToAccountId; Type: INDEX; Schema: public; Owner: nikhil
--

CREATE INDEX "IX_Transactions_ToAccountId" ON public."Transactions" USING btree ("ToAccountId");


--
-- Name: IX_Users_Email; Type: INDEX; Schema: public; Owner: nikhil
--

CREATE UNIQUE INDEX "IX_Users_Email" ON public."Users" USING btree ("Email");


--
-- Name: Accounts FK_Accounts_Users_UserId; Type: FK CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."Accounts"
    ADD CONSTRAINT "FK_Accounts_Users_UserId" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE CASCADE;


--
-- Name: Branches FK_Branches_Accounts_TreasuryAccountId; Type: FK CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."Branches"
    ADD CONSTRAINT "FK_Branches_Accounts_TreasuryAccountId" FOREIGN KEY ("TreasuryAccountId") REFERENCES public."Accounts"("Id") ON DELETE RESTRICT;


--
-- Name: Contacts FK_Contacts_Users_UserId; Type: FK CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."Contacts"
    ADD CONSTRAINT "FK_Contacts_Users_UserId" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE CASCADE;


--
-- Name: GoalContributions FK_GoalContributions_Goals_GoalId; Type: FK CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."GoalContributions"
    ADD CONSTRAINT "FK_GoalContributions_Goals_GoalId" FOREIGN KEY ("GoalId") REFERENCES public."Goals"("Id") ON DELETE CASCADE;


--
-- Name: GoalContributions FK_GoalContributions_Transactions_TransactionId; Type: FK CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."GoalContributions"
    ADD CONSTRAINT "FK_GoalContributions_Transactions_TransactionId" FOREIGN KEY ("TransactionId") REFERENCES public."Transactions"("Id") ON DELETE SET NULL;


--
-- Name: Goals FK_Goals_Accounts_GoalAccountId; Type: FK CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."Goals"
    ADD CONSTRAINT "FK_Goals_Accounts_GoalAccountId" FOREIGN KEY ("GoalAccountId") REFERENCES public."Accounts"("Id") ON DELETE RESTRICT;


--
-- Name: Goals FK_Goals_Accounts_SourceAccountId; Type: FK CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."Goals"
    ADD CONSTRAINT "FK_Goals_Accounts_SourceAccountId" FOREIGN KEY ("SourceAccountId") REFERENCES public."Accounts"("Id") ON DELETE RESTRICT;


--
-- Name: Goals FK_Goals_Users_UserId; Type: FK CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."Goals"
    ADD CONSTRAINT "FK_Goals_Users_UserId" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE CASCADE;


--
-- Name: IdempotencyKeys FK_IdempotencyKeys_Users_UserId; Type: FK CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."IdempotencyKeys"
    ADD CONSTRAINT "FK_IdempotencyKeys_Users_UserId" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE CASCADE;


--
-- Name: LedgerEntries FK_LedgerEntries_Accounts_AccountId; Type: FK CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."LedgerEntries"
    ADD CONSTRAINT "FK_LedgerEntries_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES public."Accounts"("Id") ON DELETE RESTRICT;


--
-- Name: LedgerEntries FK_LedgerEntries_Transactions_TransactionId; Type: FK CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."LedgerEntries"
    ADD CONSTRAINT "FK_LedgerEntries_Transactions_TransactionId" FOREIGN KEY ("TransactionId") REFERENCES public."Transactions"("Id") ON DELETE CASCADE;


--
-- Name: LoanRepayments FK_LoanRepayments_Loans_LoanId; Type: FK CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."LoanRepayments"
    ADD CONSTRAINT "FK_LoanRepayments_Loans_LoanId" FOREIGN KEY ("LoanId") REFERENCES public."Loans"("Id") ON DELETE CASCADE;


--
-- Name: LoanRepayments FK_LoanRepayments_Transactions_TransactionId; Type: FK CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."LoanRepayments"
    ADD CONSTRAINT "FK_LoanRepayments_Transactions_TransactionId" FOREIGN KEY ("TransactionId") REFERENCES public."Transactions"("Id") ON DELETE SET NULL;


--
-- Name: Loans FK_Loans_Accounts_FundedAccountId; Type: FK CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."Loans"
    ADD CONSTRAINT "FK_Loans_Accounts_FundedAccountId" FOREIGN KEY ("FundedAccountId") REFERENCES public."Accounts"("Id") ON DELETE RESTRICT;


--
-- Name: Loans FK_Loans_Branches_BranchId; Type: FK CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."Loans"
    ADD CONSTRAINT "FK_Loans_Branches_BranchId" FOREIGN KEY ("BranchId") REFERENCES public."Branches"("Id") ON DELETE RESTRICT;


--
-- Name: Loans FK_Loans_Transactions_DisbursementTransactionId; Type: FK CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."Loans"
    ADD CONSTRAINT "FK_Loans_Transactions_DisbursementTransactionId" FOREIGN KEY ("DisbursementTransactionId") REFERENCES public."Transactions"("Id") ON DELETE SET NULL;


--
-- Name: Loans FK_Loans_Users_UserId; Type: FK CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."Loans"
    ADD CONSTRAINT "FK_Loans_Users_UserId" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE CASCADE;


--
-- Name: Notifications FK_Notifications_Users_UserId; Type: FK CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_Users_UserId" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE CASCADE;


--
-- Name: Transactions FK_Transactions_Accounts_FromAccountId; Type: FK CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."Transactions"
    ADD CONSTRAINT "FK_Transactions_Accounts_FromAccountId" FOREIGN KEY ("FromAccountId") REFERENCES public."Accounts"("Id") ON DELETE RESTRICT;


--
-- Name: Transactions FK_Transactions_Accounts_ToAccountId; Type: FK CONSTRAINT; Schema: public; Owner: nikhil
--

ALTER TABLE ONLY public."Transactions"
    ADD CONSTRAINT "FK_Transactions_Accounts_ToAccountId" FOREIGN KEY ("ToAccountId") REFERENCES public."Accounts"("Id") ON DELETE RESTRICT;


--
-- PostgreSQL database dump complete
--

\unrestrict p8Gugexui4mvAivWd34C1fQGFxJXZZIpyA2n1QCTa0KBGJTOdaji2sShgetoFEc

