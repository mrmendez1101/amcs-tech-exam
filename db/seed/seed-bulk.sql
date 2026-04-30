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