export interface IUser {
  id: string;
  name: string;
  username: string;
  emailAddress: string;
  role: string;
}

export interface ISecretKey {
  id: string;
  keyType: number;
  key: string;
  description: string;
  isApproved: boolean;
  isConnected: boolean;
}

export type Nullable<T> = {
  [P in keyof T]: T[P] | null;
};