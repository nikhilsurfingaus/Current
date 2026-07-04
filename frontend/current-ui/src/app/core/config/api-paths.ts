export const API_PATHS = {
  auth: {
    login: '/auth/login',
    register: '/auth/register',
  },
  users: {
    me: '/users/me',
    byId: (userId: string) => `/users/${userId}`,
  },
  accounts: {
    list: '/accounts',
    byId: (accountId: string) => `/accounts/${accountId}`,
  },
  transactions: {
    list: '/transactions',
    byId: (transactionId: string) => `/transactions/${transactionId}`,
    transfer: '/transactions/transfer',
  },
} as const;
