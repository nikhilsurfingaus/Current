import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, signal } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { RouterLink } from '@angular/router';

import { ContactService } from '../../../core/services/contact.service';
import { ToastService } from '../../../core/services/toast.service';
import {
  Contact,
  CreateContactRequest,
  UpdateContactRequest,
} from '../../../shared/models';
import { resolveApiErrorMessage } from '../../../shared/utils/http-error.utils';

@Component({
  selector: 'app-contacts',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './contacts.html',
  styleUrl: './contacts.scss',
})
export class ContactsComponent implements OnInit {
  contacts = signal<Contact[]>([]);
  contactsLoading = signal(false);
  contactsLoadError = signal('');
  contactFormSubmitted = signal(false);
  contactRequestInFlight = signal(false);
  contactErrorMessage = signal('');
  editingContactId = signal<string | null>(null);
  pendingDeleteContactId = signal<string | null>(null);
  deletingContactId = signal<string | null>(null);

  contactForm = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email, Validators.maxLength(255)],
    }),
  });

  constructor(
    private contactService: ContactService,
    private toastService: ToastService,
  ) {}

  ngOnInit(): void {
    this.loadContacts();
  }

  loadContacts(): void {
    this.contactsLoading.set(true);
    this.contactsLoadError.set('');

    this.contactService.getAllContacts().subscribe({
      next: (contacts) => {
        this.contacts.set(contacts);
        this.contactsLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.contactsLoading.set(false);
        this.contactsLoadError.set(
          resolveApiErrorMessage(error, 'Unable to load contacts.'),
        );
      },
    });
  }

  startEditing(contact: Contact): void {
    this.editingContactId.set(contact.id);
    this.pendingDeleteContactId.set(null);
    this.contactErrorMessage.set('');
    this.contactFormSubmitted.set(false);
    this.contactForm.setValue({
      name: contact.name,
      email: contact.email,
    });
  }

  cancelEditing(): void {
    this.resetForm();
  }

  onSubmitContact(): void {
    this.contactFormSubmitted.set(true);
    this.contactErrorMessage.set('');

    if (this.contactForm.invalid) {
      return;
    }

    const formValues = this.contactForm.getRawValue();
    const contactRequest = {
      name: formValues.name.trim(),
      email: formValues.email.trim(),
    };
    const editingContactId = this.editingContactId();

    this.contactRequestInFlight.set(true);

    if (editingContactId) {
      this.updateContact(editingContactId, contactRequest);
      return;
    }

    this.createContact(contactRequest);
  }

  requestDelete(contactId: string): void {
    this.pendingDeleteContactId.set(contactId);
  }

  cancelDelete(): void {
    this.pendingDeleteContactId.set(null);
  }

  confirmDelete(contactId: string): void {
    this.deletingContactId.set(contactId);

    this.contactService.deleteContact(contactId).subscribe({
      next: () => {
        this.contacts.update((contacts) => contacts.filter((contact) => contact.id !== contactId));
        this.deletingContactId.set(null);
        this.pendingDeleteContactId.set(null);
        this.toastService.showSuccess('Contact deleted.');
      },
      error: (error: HttpErrorResponse) => {
        this.deletingContactId.set(null);
        this.contactErrorMessage.set(
          resolveApiErrorMessage(error, 'Unable to delete contact.'),
        );
      },
    });
  }

  private createContact(request: CreateContactRequest): void {
    this.contactService.createContact(request).subscribe({
      next: (createdContact) => {
        this.contacts.update((contacts) =>
          [...contacts, createdContact].sort((left, right) => left.name.localeCompare(right.name)),
        );
        this.contactRequestInFlight.set(false);
        this.toastService.showSuccess('Contact saved.');
        this.resetForm();
      },
      error: (error: HttpErrorResponse) => {
        this.contactRequestInFlight.set(false);
        this.contactErrorMessage.set(
          resolveApiErrorMessage(error, 'Unable to save contact.'),
        );
      },
    });
  }

  private updateContact(contactId: string, request: UpdateContactRequest): void {
    this.contactService.updateContact(contactId, request).subscribe({
      next: (updatedContact) => {
        this.contacts.update((contacts) =>
          contacts
            .map((contact) => contact.id === updatedContact.id ? updatedContact : contact)
            .sort((left, right) => left.name.localeCompare(right.name)),
        );
        this.contactRequestInFlight.set(false);
        this.toastService.showSuccess('Contact updated.');
        this.resetForm();
      },
      error: (error: HttpErrorResponse) => {
        this.contactRequestInFlight.set(false);
        this.contactErrorMessage.set(
          resolveApiErrorMessage(error, 'Unable to update contact.'),
        );
      },
    });
  }

  private resetForm(): void {
    this.editingContactId.set(null);
    this.contactFormSubmitted.set(false);
    this.contactErrorMessage.set('');
    this.contactForm.reset({
      name: '',
      email: '',
    });
  }
}
