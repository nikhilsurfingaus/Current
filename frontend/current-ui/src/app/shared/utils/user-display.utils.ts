export function getTimeGreeting(): string {
  const hour = new Date().getHours();

  if (hour < 12) {
    return 'Good morning';
  }

  if (hour < 17) {
    return 'Good afternoon';
  }

  return 'Good evening';
}

export function getDisplayNameFromEmail(email: string): string {
  const localPart = email.split('@')[0] ?? '';
  const namePart = localPart.split(/[._-]/)[0] ?? localPart;

  if (!namePart) {
    return 'there';
  }

  return namePart.charAt(0).toUpperCase() + namePart.slice(1);
}

export function getUserInitialsFromNames(firstName: string, lastName: string): string {
  const firstInitial = firstName.charAt(0);
  const lastInitial = lastName.charAt(0);

  if (firstInitial && lastInitial) {
    return `${firstInitial}${lastInitial}`.toUpperCase();
  }

  return (firstInitial || lastInitial || '?').toUpperCase();
}
