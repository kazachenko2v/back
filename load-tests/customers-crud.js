import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = (__ENV.BASE_URL || 'http://localhost:8080').replace(/\/$/, '');
const VUS = Number(__ENV.VUS || 5);
const DURATION = __ENV.DURATION || '30s';
const SLEEP_SECONDS = Number(__ENV.SLEEP || 1);

export const options = {
  scenarios: {
    customer_crud: {
      executor: 'constant-vus',
      vus: VUS,
      duration: DURATION,
      gracefulStop: '10s',
    },
  },
  thresholds: {
    http_req_failed: [__ENV.FAIL_RATE || 'rate<0.01'],
    http_req_duration: [__ENV.P95 || 'p(95)<700'],
    checks: ['rate>0.99'],
  },
};

export default function () {
  const uniqueId = `${Date.now()}-${__VU}-${__ITER}`;
  const createPayload = JSON.stringify({
    name: `Load Customer ${uniqueId}`,
    email: `load-${uniqueId}@example.test`,
  });

  const createResponse = http.post(`${BASE_URL}/api/customers`, createPayload, {
    headers: { 'Content-Type': 'application/json' },
    tags: { endpoint: 'create_customer' },
  });

  let customer = {};
  try {
    customer = createResponse.json();
  } catch {
    customer = {};
  }

  const created = check(createResponse, {
    'create status is 201': (res) => res.status === 201,
    'create returned id': () => typeof customer.id === 'string' && customer.id.length > 0,
  });

  if (!created || !customer.id) {
    sleep(SLEEP_SECONDS);
    return;
  }

  const updateResponse = http.patch(
    `${BASE_URL}/api/customers/${customer.id}/email`,
    JSON.stringify({ email: `updated-${uniqueId}@example.test` }),
    {
      headers: { 'Content-Type': 'application/json' },
      tags: { endpoint: 'change_customer_email' },
    },
  );

  check(updateResponse, {
    'update status is 200': (res) => res.status === 200,
  });

  const deleteResponse = http.del(`${BASE_URL}/api/customers/${customer.id}`, null, {
    tags: { endpoint: 'delete_customer' },
  });

  check(deleteResponse, {
    'delete status is 204': (res) => res.status === 204,
  });

  sleep(SLEEP_SECONDS);
}
