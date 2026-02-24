import ky from 'ky';

export const api = ky.create({
  headers: { Accept: 'application/json' }
});