/**
 * Strips leading zeros from amount input text while preserving values like "0" and "0.50".
 * Examples: "0564.65" -> "564.65", "007" -> "7", "00.5" -> "0.5"
 */
export function normalizeAmountInputValue(rawValue: string): string {
  if (!rawValue) {
    return rawValue;
  }

  const decimalIndex = rawValue.indexOf('.');
  const hasDecimal = decimalIndex !== -1;
  const integerPart = hasDecimal ? rawValue.slice(0, decimalIndex) : rawValue;
  const decimalPart = hasDecimal ? rawValue.slice(decimalIndex + 1) : '';

  const normalizedInteger = integerPart.replace(/^0+(?=\d)/, '');

  if (hasDecimal) {
    return `${normalizedInteger || '0'}.${decimalPart}`;
  }

  return normalizedInteger;
}
