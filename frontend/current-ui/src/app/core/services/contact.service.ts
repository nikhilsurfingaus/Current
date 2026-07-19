import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { API_PATHS } from '../config/api-paths';
import { ApiService } from './api.service';
import {
  Contact,
  CreateContactRequest,
  UpdateContactRequest,
} from '../../shared/models';

@Injectable({
  providedIn: 'root',
})
export class ContactService {
  constructor(private apiService: ApiService) {}

  getAllContacts(): Observable<Contact[]> {
    return this.apiService.get<Contact[]>(API_PATHS.contacts.list);
  }

  createContact(request: CreateContactRequest): Observable<Contact> {
    return this.apiService.post<Contact>(API_PATHS.contacts.list, request);
  }

  updateContact(contactId: string, request: UpdateContactRequest): Observable<Contact> {
    return this.apiService.put<Contact>(API_PATHS.contacts.byId(contactId), request);
  }

  deleteContact(contactId: string): Observable<void> {
    return this.apiService.delete<void>(API_PATHS.contacts.byId(contactId));
  }
}
