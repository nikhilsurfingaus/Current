export interface Contact {
  id: string;
  name: string;
  email?: string | null;
  bsb?: string | null;
  accountNumber?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateContactRequest {
  name: string;
  email?: string | null;
  bsb?: string | null;
  accountNumber?: string | null;
}

export interface UpdateContactRequest {
  name: string;
  email?: string | null;
  bsb?: string | null;
  accountNumber?: string | null;
}
