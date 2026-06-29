# Load Tests

k6 scripts for the customer API.

## Run With Docker

Use `host.docker.internal` when k6 itself runs in Docker and the API is exposed on
the Windows host.

```powershell
cd D:\projects\back
docker run --rm -v "${PWD}\load-tests:/scripts" grafana/k6 run -e BASE_URL=http://host.docker.internal:8080 /scripts/customers-read.js
```

## Seed 1000 Customers

This creates customers and leaves them in the database.

```powershell
docker run --rm -v "${PWD}\load-tests:/scripts" grafana/k6 run -e BASE_URL=http://host.docker.internal:8080 -e TARGET_COUNT=1000 /scripts/seed-customers.js
```

## Read Stress Test

```powershell
docker run --rm -v "${PWD}\load-tests:/scripts" grafana/k6 run -e BASE_URL=http://host.docker.internal:8080 -e VUS=25 -e DURATION=1m /scripts/customers-read.js
```

Run it twice to compare Redis cold cache vs warm cache. The first run should have
more `X-Cache: MISS` responses. The second run should have mostly
`X-Cache: HIT` responses.

Warm-cache pass rule example:

```powershell
docker run --rm -v "${PWD}\load-tests:/scripts" grafana/k6 run -e BASE_URL=http://host.docker.internal:8080 -e VUS=50 -e DURATION=1m -e PAGE_SIZE=50 -e MAX_PAGE=5 -e SLEEP=0.1 -e CACHE_HIT_RATE="rate>0.95" /scripts/customers-read.js
```

## CRUD Stress Test

This creates, updates, and deletes customers during the test.

```powershell
docker run --rm -v "${PWD}\load-tests:/scripts" grafana/k6 run -e BASE_URL=http://host.docker.internal:8080 -e VUS=10 -e DURATION=1m /scripts/customers-crud.js
```

## Useful Environment Variables

- `BASE_URL`: API base URL.
- `VUS`: number of virtual users.
- `DURATION`: test duration, such as `30s`, `1m`, or `5m`.
- `PAGE_SIZE`: page size for read test.
- `MAX_PAGE`: highest page number the read test cycles through.
- `P95`: p95 duration threshold, such as `p(95)<500`.
- `FAIL_RATE`: failed request threshold, such as `rate<0.01`.
