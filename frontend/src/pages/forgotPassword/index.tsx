import { AuthPage } from "@refinedev/mui";
import { Logo } from "../../components";

export const ForgotPassword = () => {
  return <AuthPage type="forgotPassword" title={<Logo />} />;
};
