export const APP_BRAND_NAME = 'Current';

export const APP_TITLE_TAGLINE = 'Accounts, transfers, transactions & more';

export const AUTH_TITLE_ROUTE_DATA_KEY = 'authMarketingTitle';

export function buildMarketingDocumentTitle(pageTitle?: string): string {
  if (!pageTitle) {
    return `${APP_BRAND_NAME} – ${APP_TITLE_TAGLINE} | ${APP_BRAND_NAME}`;
  }

  return `${pageTitle} – ${APP_TITLE_TAGLINE} | ${APP_BRAND_NAME}`;
}

export function buildAppDocumentTitle(pageTitle: string): string {
  return `${APP_BRAND_NAME} – ${pageTitle}`;
}
