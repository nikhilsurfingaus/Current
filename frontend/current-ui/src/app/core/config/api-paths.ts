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
  goals: {
    list: '/goals',
    byId: (goalId: string) => `/goals/${goalId}`,
    contribute: (goalId: string) => `/goals/${goalId}/contribute`,
    withdraw: (goalId: string) => `/goals/${goalId}/withdraw`,
    history: (goalId: string) => `/goals/${goalId}/history`,
  },
  analytics: {
    overview: '/analytics/overview',
    cashFlow: '/analytics/cashflow',
    netWorthHistory: '/analytics/networth-history',
    categories: '/analytics/categories',
    goals: '/analytics/goals',
    monthlySummary: '/analytics/monthly-summary',
  },
  payments: {
    send: '/payments/send',
    sent: '/payments/sent',
    received: '/payments/received',
    history: '/payments/history',
    byId: (transactionId: string) => `/payments/${transactionId}`,
  },
} as const;
