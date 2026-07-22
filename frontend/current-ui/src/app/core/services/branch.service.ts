import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { API_PATHS } from '../config/api-paths';
import { ApiService } from './api.service';
import {
  BranchDisbursement,
  BranchTreasury,
  CreateBranchDisbursementRequest,
} from '../../shared/models/branches/branch.model';

@Injectable({
  providedIn: 'root',
})
export class BranchService {
  constructor(private apiService: ApiService) {}

  getTreasury(): Observable<BranchTreasury> {
    return this.apiService.get<BranchTreasury>(API_PATHS.branch.treasury);
  }

  createDisbursement(request: CreateBranchDisbursementRequest): Observable<BranchDisbursement> {
    return this.apiService.post<BranchDisbursement>(API_PATHS.branch.disbursements, request);
  }
}
