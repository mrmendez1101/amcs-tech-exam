# Seed Data

Two SQL scripts for populating the marketplace DB. The small demo seed gives you enough rows to exercise endpoints and run curl examples. The bulk seed creates 100K+ rows so you can run `EXPLAIN ANALYZE` and prove your indexes earn their keep at scale.

Both assume the schema from the `0001`-`0005` migrations is already in place.

## Table of contents

- [Seed Data](#seed-data)
  - [Table of contents](#table-of-contents)
  - [Where these files live](#where-these-files-live)
  - [Running the scripts](#running-the-scripts)
  - [Small demo seed](#small-demo-seed)
  - [Bulk seed for scale testing](#bulk-seed-for-scale-testing)
  - [Verification queries](#verification-queries)
  - [Resetting between runs](#resetting-between-runs)
  - [Why SQL scripts and not a C# seeder](#why-sql-scripts-and-not-a-c-seeder)

## Where these files live

Create both as plain `.sql` files in the repo (not embedded in the API assembly):

```
db/
  seed/
    seed-demo.sql
    seed-bulk.sql
```

These are NOT migration scripts. Migrations live in `src/Marketplace.Api/Shared/Migrations/Scripts/` and run automatically on startup. Seed scripts run manually, on demand, against a running Postgres container.

## Running the scripts

After `docker compose up` is running and migrations have applied:

```bash
# Demo seed (a few hundred rows, runs in seconds)
docker exec -i marketplace_postgres \
  psql -U marketplace -d marketplace < db/seed/seed-demo.sql

# Bulk seed (110K+ rows, runs in a few minutes)
docker exec -i marketplace_postgres \
  psql -U marketplace -d marketplace < db/seed/seed-bulk.sql
```

The `-i` flag is essential: it pipes the file's stdin into the container's `psql`. Without it, the redirect is silently ignored.

If you prefer to run from inside the container interactively:

```bash
docker exec -it marketplace_postgres psql -U marketplace -d marketplace
\i /path/inside/container/seed-demo.sql
```

To pipe to that path you would need to mount it in `docker-compose.yml`. Easier to use the `<` redirect from the host.

## Small demo seed

Save this as `db/seed/seed-demo.sql`. It creates roughly 500 customers, 50 contractors, 200 jobs, and 300 job offers (most jobs have 1-2 offers, a few have 0). Names cycle through small arrays via modular arithmetic so the result feels varied without needing a names library.

```sql
-- db/seed/seed-demo.sql
-- Small demo seed for local development and curl examples.
-- Roughly: 500 customers, 50 contractors, 200 jobs, ~300 offers.
-- Idempotent-ish: wrap in a transaction and run TRUNCATE first if you want a clean slate.

BEGIN;

-- Customers --------------------------------------------------------------
INSERT INTO customers (id, first_name, last_name)
SELECT
    gen_random_uuid(),
    (ARRAY[
        'John','Jane','Bob','Alice','Carlos','Maria','Hiroshi','Yuki',
        'Pierre','Marie','Liam','Emma','Noah','Olivia','Mateo','Sofia',
        'Arjun','Priya','Chen','Mei'
    ])[1 + (i % 20)] AS first_name,
    (ARRAY[
        'Smith','Jones','Garcia','Tanaka','Dubois','Schmidt','Patel','Singh',
        'Brown','Wilson','Anderson','Nguyen','Lopez','Kim','Johansson','Rossi',
        'Murphy','Cohen','Reyes','Khan','Andersen','Walker','Hall','Young','Lee'
    ])[1 + ((i / 20) % 25)] AS last_name
FROM generate_series(1, 500) AS i;

-- Contractors ------------------------------------------------------------
INSERT INTO contractors (id, name, rating)
SELECT
    gen_random_uuid(),
    (ARRAY[
        'Acme','Bright','Stellar','Summit','Apex','Vertex','Pioneer','Nimbus',
        'Cobalt','Granite','Ironhand','Mosaic'
    ])[1 + (i % 12)]
        || ' ' ||
    (ARRAY[
        'Builders','Plumbing','Electric','Roofing','Painting','Landscaping',
        'Carpentry','Renovations','Services','Contracting','Works','Trades'
    ])[1 + ((i / 12) % 12)],
    -- Ratings between 3.0 and 5.0, rounded to one decimal.
    ROUND((3.0 + (random() * 2.0))::numeric, 1)
FROM generate_series(1, 50) AS i;

-- Jobs -------------------------------------------------------------------
-- Each job is created by a randomly chosen customer.
WITH chosen_customers AS (
    SELECT id FROM customers ORDER BY random() LIMIT 200
),
numbered AS (
    SELECT id, ROW_NUMBER() OVER () AS rn FROM chosen_customers
)
INSERT INTO jobs (id, customer_id, start_date, due_date, budget, description, status)
SELECT
    gen_random_uuid(),
    n.id,
    CURRENT_DATE + ((n.rn % 14))::int,                 -- starts in the next 2 weeks
    CURRENT_DATE + ((n.rn % 14) + 7 + (n.rn % 21))::int, -- due 1-4 weeks after start
    (500 + (random() * 4500))::numeric(10, 2),         -- budget 500-5000
    (ARRAY[
        'Repaint the back fence',
        'Install three ceiling fans',
        'Replace kitchen tap and check leak',
        'Mow lawn and trim hedges',
        'Patch and repaint hallway wall',
        'Service air conditioning unit',
        'Build raised garden bed',
        'Tile small bathroom floor',
        'Rewire garage lighting',
        'Pressure wash driveway'
    ])[1 + (n.rn % 10)],
    'Open'
FROM numbered n;

-- Job offers -------------------------------------------------------------
-- ~1-2 offers per job, from random contractors, priced near (but not exactly) the budget.
WITH job_pool AS (
    SELECT id, budget FROM jobs
),
contractor_pool AS (
    SELECT id FROM contractors
),
pairs AS (
    SELECT
        j.id      AS job_id,
        j.budget  AS budget,
        c.id      AS contractor_id,
        random()  AS r
    FROM job_pool j
    CROSS JOIN contractor_pool c
)
INSERT INTO job_offers (id, job_id, contractor_id, price, created_at)
SELECT
    gen_random_uuid(),
    p.job_id,
    p.contractor_id,
    (p.budget * (0.85 + (p.r * 0.30)))::numeric(10, 2), -- 85%-115% of budget
    now() - (random() * interval '7 days')
FROM (
    SELECT job_id, contractor_id, budget, r,
           ROW_NUMBER() OVER (PARTITION BY job_id ORDER BY r) AS rn
    FROM pairs
) p
WHERE p.rn <= 1 + (CASE WHEN random() < 0.4 THEN 1 ELSE 0 END);

COMMIT;

-- Verification (run separately to inspect)
-- SELECT COUNT(*) AS customers FROM customers;
-- SELECT COUNT(*) AS contractors FROM contractors;
-- SELECT COUNT(*) AS jobs FROM jobs;
-- SELECT COUNT(*) AS offers FROM job_offers;
```

Adjust the column names to match your actual schema. The script assumes:

* `customers (id uuid, first_name text, last_name text)`
* `contractors (id uuid, name text, rating numeric)`
* `jobs (id uuid, customer_id uuid, start_date date, due_date date, budget numeric, description text, status text)`
* `job_offers (id uuid, job_id uuid, contractor_id uuid, price numeric, created_at timestamptz)`

If your migrations name columns differently (e.g., `business_name` instead of `name`), update the script. Don't update the migrations to fit the seed.

## Bulk seed for scale testing

Save as `db/seed/seed-bulk.sql`. This creates 100,000 customers and 10,000 contractors via `generate_series`, which is fast (a few minutes on a laptop) because it's a single bulk insert per table.

```sql
-- db/seed/seed-bulk.sql
-- Bulk seed for scale testing and EXPLAIN ANALYZE.
-- 100,000 customers, 10,000 contractors. No jobs/offers (add separately if needed).

BEGIN;

-- Customers --------------------------------------------------------------
INSERT INTO customers (id, first_name, last_name)
SELECT
    gen_random_uuid(),
    (ARRAY[
        'John','Jane','Bob','Alice','Carlos','Maria','Hiroshi','Yuki',
        'Pierre','Marie','Liam','Emma','Noah','Olivia','Mateo','Sofia',
        'Arjun','Priya','Chen','Mei','David','Sarah','James','Linda',
        'Robert','Patricia','Michael','Jennifer','William','Elizabeth'
    ])[1 + (i % 30)],
    (ARRAY[
        'Smith','Jones','Garcia','Tanaka','Dubois','Schmidt','Patel','Singh',
        'Brown','Wilson','Anderson','Nguyen','Lopez','Kim','Johansson','Rossi',
        'Murphy','Cohen','Reyes','Khan','Andersen','Walker','Hall','Young','Lee',
        'Martinez','Robinson','Clark','Rodriguez','Lewis','Walker','Allen',
        'Sanchez','Wright','King','Scott','Torres','Hill','Adams','Baker',
        'Nelson','Carter','Mitchell','Perez','Roberts','Turner','Phillips',
        'Campbell','Parker','Evans','Edwards','Collins','Stewart','Morris'
    ])[1 + ((i / 30) % 53)]
FROM generate_series(1, 100000) AS i;

-- Contractors ------------------------------------------------------------
INSERT INTO contractors (id, name, rating)
SELECT
    gen_random_uuid(),
    'Contractor ' || lpad(i::text, 5, '0') || ' ' ||
    (ARRAY[
        'Builders','Plumbing','Electric','Roofing','Painting','Landscaping',
        'Carpentry','Renovations','Services','Contracting','Works','Trades',
        'Solutions','Group','Co','Partners','Specialists','Crew'
    ])[1 + (i % 18)],
    ROUND((1.0 + (random() * 4.0))::numeric, 1)
FROM generate_series(1, 10000) AS i;

COMMIT;

-- After running this script, refresh planner stats so EXPLAIN reflects reality:
ANALYZE customers;
ANALYZE contractors;
```

The contractor names use `lpad` to guarantee uniqueness. The customer names intentionally collide (multiple John Smiths exist) so prefix search returns realistic batches.

To push customers to 1M for a sterner test, change `generate_series(1, 100000)` to `generate_series(1, 1000000)`. Expect 30-90 seconds depending on disk speed. To push to 10M, run in batches and consider disabling indexes during insert, then rebuilding them.

## Verification queries

After seeding, run these to confirm the indexes are doing their job. Each `EXPLAIN ANALYZE` should show an `Index Scan` or `Bitmap Index Scan`, never a `Seq Scan` on a 100K-row table.

```sql
-- Should hit the btree on last_name
EXPLAIN ANALYZE
SELECT id, first_name, last_name
FROM customers
WHERE last_name ILIKE 'smi%'
ORDER BY last_name, first_name
LIMIT 20;

-- Should hit the trigram GIN index for contains-anywhere search
EXPLAIN ANALYZE
SELECT id, first_name, last_name
FROM customers
WHERE last_name ILIKE '%mit%'
LIMIT 20;

-- Should hit the primary key
EXPLAIN ANALYZE
SELECT * FROM customers WHERE id = '<<paste-a-real-uuid>>';

-- Distribution sanity check: should not all be one bucket
SELECT last_name, COUNT(*)
FROM customers
GROUP BY last_name
ORDER BY COUNT(*) DESC
LIMIT 10;
```

If a query returns `Seq Scan`, either the index is missing (check the `0005_add_search_indexes.sql` migration applied), or the planner thinks a scan is cheaper (run `ANALYZE customers;` to refresh stats).

## Resetting between runs

To clear seeded data without dropping the schema:

```sql
TRUNCATE job_offers, jobs, contractors, customers RESTART IDENTITY CASCADE;
```

To wipe everything including the schema and start over:

```bash
docker compose down -v
docker compose up --build
# migrations rerun, then seed again
```

`down -v` removes the named volume `pgdata`. Without `-v`, the data persists across `down`/`up`.

## Why SQL scripts and not a C# seeder

Can write a `Seeder.cs` class run from `Program.cs` behind a config flag. (*Options)

For an exam:
* SQL scripts are reviewable as data, not as code that produces data. Can check exactly what is in the DB.
* `generate_series` runs entirely server-side; it is faster than 100K round trips from a C# loop, and faster than batched inserts.
* It separates the bonus (working API) from the demo (running it with realistic data). The API does not need to know the seed exists.
* If you do want a programmatic seeder later (e.g., for integration tests), wire one in `tests/`. Keep it out of the production startup path.

The one case where a C# seeder is better: when seed data needs to invoke domain logic (e.g., `Job.AcceptOffer()` to set up an awarded job). For that, write a small console project under `tools/Seeder/` that calls into the domain assembly. Keep production startup clean.
