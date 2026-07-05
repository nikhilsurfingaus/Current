import { Injectable, inject } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRouteSnapshot, RouterStateSnapshot, TitleStrategy } from '@angular/router';

import {
  AUTH_TITLE_ROUTE_DATA_KEY,
  buildAppDocumentTitle,
  buildMarketingDocumentTitle,
} from './app-title.constants';

@Injectable()
export class AppTitleStrategy extends TitleStrategy {
  private readonly documentTitle = inject(Title);

  override updateTitle(snapshot: RouterStateSnapshot): void {
    const routeTitle = this.buildTitle(snapshot);
    const usesMarketingTitle = this.routeUsesMarketingTitle(snapshot.root);

    if (usesMarketingTitle) {
      this.documentTitle.setTitle(buildMarketingDocumentTitle(routeTitle ?? undefined));
      return;
    }

    this.documentTitle.setTitle(buildAppDocumentTitle(routeTitle ?? 'Dashboard'));
  }

  private routeUsesMarketingTitle(route: ActivatedRouteSnapshot): boolean {
    if (route.data[AUTH_TITLE_ROUTE_DATA_KEY] === true) {
      return true;
    }

    for (const childRoute of route.children) {
      if (this.routeUsesMarketingTitle(childRoute)) {
        return true;
      }
    }

    return false;
  }
}
