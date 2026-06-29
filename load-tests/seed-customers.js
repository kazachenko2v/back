import http from 'k6/http';
import { check, fail } from 'k6';

const BASE_URL = (__ENV.BASE_URL || 'http://localhost:8080').replace(/\/$/, '');
const TARGET_COUNT = Number(__ENV.TARGET_COUNT || 1000);
const RUN_ID = __ENV.RUN_ID || Date.now();

export const options = {
  vus: 1,
  iterations: 1,
  thresholds: {
    checks: ['rate==1'],
  },
};

export default function () {
  const countResponse = http.get(`${BASE_URL}/api/customers?page=1&pageSize=1`);
  if (!check(countResponse, { 'count request status is 200': (res) => res.status === 200 })) {
    fail(`Cannot read current customer count from ${BASE_URL}.`);
  }

  const currentCount = Number(countResponse.json('totalCount') || 0);
  if (currentCount >= TARGET_COUNT) {
    console.log(`Current customer count is ${currentCount}; target is ${TARGET_COUNT}. Nothing to seed.`);
    return;
  }

  const customersToCreate = TARGET_COUNT - currentCount;
  console.log(`Creating ${customersToCreate} customers to reach ${TARGET_COUNT}.`);

  for (let index = currentCount + 1; index <= TARGET_COUNT; index += 1) {
    const response = http.post(
      `${BASE_URL}/api/customers`,
      JSON.stringify({
        name: `Load Customer ${index}`,
        email: `load-${RUN_ID}-${index}@example.test`,
      }),
      {
        headers: { 'Content-Type': 'application/json' },
        tags: { endpoint: 'seed_customer' },
      },
    );

    if (!check(response, { 'seed create status is 201': (res) => res.status === 201 })) {
      fail(`Failed to create seed customer ${index}: ${response.status} ${response.body}`);
    }
  }
}
