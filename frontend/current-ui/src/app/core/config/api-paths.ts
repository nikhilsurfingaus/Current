export const API_PATHS = {
  auth: {
    login: '/auth/login',
    register: '/auth/register',
    verifyEmail: '/auth/verify-email',
    resendVerification: '/auth/resend-verification',
  },
  users: {
    me: '/users/me',
    profile: '/users/me/profile',
    preferences: '/users/me/preferences',
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
  contacts: {
    list: '/contacts',
    byId: (contactId: string) => `/contacts/${contactId}`,
  },
  branch: {
    treasury: '/branch/treasury',
    disbursements: '/branch/disbursements',
    loans: '/branch/loans',
    approveLoan: (loanId: string) => `/branch/loans/${loanId}/approve`,
    rejectLoan: (loanId: string) => `/branch/loans/${loanId}/reject`,
  },
  loans: {
    list: '/loans',
    limits: '/loans/limits',
    byId: (loanId: string) => `/loans/${loanId}`,
    repay: (loanId: string) => `/loans/${loanId}/repay`,
    repayments: (loanId: string) => `/loans/${loanId}/repayments`,
  },
  notifications: {
    list: '/notifications',
    unreadCount: '/notifications/unread-count',
    markRead: (notificationId: string) => `/notifications/${notificationId}/read`,
    markAllRead: '/notifications/read-all',
  },
} as const;
