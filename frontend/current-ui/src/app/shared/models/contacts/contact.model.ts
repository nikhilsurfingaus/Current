export interface Contact {
  id: string;
  name: string;
  email: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateContactRequest {
  name: string;
  email: string;
}

export interface UpdateContactRequest {
  name: string;
  email: string;
}
