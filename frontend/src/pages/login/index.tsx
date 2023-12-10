import { AuthPage } from "@refinedev/mui";
import { Logo } from "../../components";

export const Login = () => {
  return (
    <AuthPage
      type="login"
      title={<Logo />}
      formProps={{
        defaultValues: { email: "", password: "" },
      }}
    />
  );
};
