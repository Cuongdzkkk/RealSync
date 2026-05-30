export const isValidEmail = (value: string): boolean => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);

export const isValidPhone = (value: string): boolean => /^(0|\+84)[0-9]{9,10}$/.test(value.replace(/\s/g, ''));
