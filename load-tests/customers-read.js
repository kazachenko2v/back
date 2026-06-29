import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

const BASE_URL = (__ENV.BASE_URL || 'http://localhost:8080').replace(/\/$/, '');
const PAGE_SIZE = Number(__ENV.PAGE_SIZE || 50);
const MAX_PAGE = Number(__ENV.MAX_PAGE || 20);
const VUS = Number(__ENV.VUS || 10);
const DURATION = __ENV.DURATION || '30s';
const SLEEP_SECONDS = Number(__ENV.SLEEP || 1);

export const cacheHitRate = new Rate('cache_hit_rate');
export const cacheMissRate = new Rate('cache_miss_rate');

export const options = {
  scenarios: {
    read_customers: {
      executor: 'constant-vus',
      vus: VUS,
      duration: DURATION,
      gracefulStop: '5s',
    },
  },
  thresholds: {
    http_req_failed: [__ENV.FAIL_RATE || 'rate<0.01'],
    http_req_duration: [__ENV.P95 || 'p(95)<500'],
    checks: ['rate>0.99'],
    cache_hit_rate: [__ENV.CACHE_HIT_RATE || 'rate>=0'],
  },
};

export default function () {
  const page = (__ITER % MAX_PAGE) + 1;
  const response = http.get(`${BASE_URL}/api/customers?page=${page}&pageSize=${PAGE_SIZE}`, {
    tags: { endpoint: 'list_customers' },
  });

  let body = {};
  try {
    body = response.json();
  } catch {
    body = {};
  }

  const cacheHeader = response.headers['X-Cache'];
  cacheHitRate.add(cacheHeader === 'HIT');
  cacheMissRate.add(cacheHeader === 'MISS');

  check(response, {
    'status is 200': (res) => res.status === 200,
    'cache header exists': () => cacheHeader === 'HIT' || cacheHeader === 'MISS',
    'items is array': () => Array.isArray(body.items),
    'page metadata exists': () =>
      Number.isInteger(body.page) &&
      Number.isInteger(body.pageSize) &&
      Number.isInteger(body.totalCount),
  });

  sleep(SLEEP_SECONDS);
}
