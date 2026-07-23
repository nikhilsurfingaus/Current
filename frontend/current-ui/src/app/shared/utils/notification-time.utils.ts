export function formatNotificationRelativeTime(isoDate: string): string {
  const createdAt = new Date(isoDate);
  const elapsedMs = Date.now() - createdAt.getTime();

  if (Number.isNaN(elapsedMs)) {
    return '';
  }

  const elapsedMinutes = Math.floor(elapsedMs / 60000);

  if (elapsedMinutes < 1) {
    return 'Just now';
  }

  if (elapsedMinutes < 60) {
    return `${elapsedMinutes} min ago`;
  }

  const elapsedHours = Math.floor(elapsedMinutes / 60);

  if (elapsedHours < 24) {
    return `${elapsedHours} hr ago`;
  }

  const elapsedDays = Math.floor(elapsedHours / 24);

  if (elapsedDays === 1) {
    return 'Yesterday';
  }

  if (elapsedDays < 7) {
    return `${elapsedDays} days ago`;
  }

  return createdAt.toLocaleDateString();
}
