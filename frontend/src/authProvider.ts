import { AuthBindings } from "@refinedev/core";
import { AxiosInstance } from "axios";

export const TOKEN_KEY = "refine-auth";

export function authProvider(url: string, axiosInstance: AxiosInstance): AuthBindings {
  return {
    login: async ({ username, email, password, remember }) => {
      if ((username || email) && password) {
        try {
          const { status } = await axiosInstance.post(url + '/login?useCookies=true&useSessionCookies=' + !!remember, {
            email: username || email,
            password,
          });
          if (status === 200) {
            return {
              success: true,
              redirectTo: "/",
            };
          }
        } catch {
        }
      }

      return {
        success: false,
        error: {
          name: "LoginError",
          message: "Invalid username or password",
        },
      };
    },
    logout: async () => {
      const { status } = await axiosInstance.post(url + '/logout');
      if (status === 200) {
        return {
          success: true,
          redirectTo: "/",
        };
      }
      return {
        success: false,
      };
    },
    check: async () => {
      const { status, headers } = await axiosInstance.get(url + '/manage/info');
      console.log(status, headers)
      if (status === 200 && headers['content-type'].startsWith('application/json')) {
        return {
          authenticated: true,
        };
      }

      return {
        authenticated: false,
        redirectTo: "/login",
      };
    },
    getPermissions: async () => null,
    getIdentity: async () => {
      const { status, headers, data } = await axiosInstance.get(url + '/manage/info');
      if (status === 200 && headers['content-type'].startsWith('application/json')) {
        console.log(data)
        return {
          id: data.id,
          username: data.username,
          name: data.name,
          email: data.email,
          avatar: "https://i.pravatar.cc/300",
          role: data.role,
        };
      }
      return null;
    },
    onError: async (error) => {
      console.error(error);
      return { error };
    },
    register: async ({ username, email, password }) => {
      if ((username || email) && password) {
        try {
          const { status } = await axiosInstance.post(url + '/register', {
            email: username || email,
            password,
          });
          if (status === 204) {
            return {
              success: true,
              redirectTo: "/",
            };
          }
        } catch {
        }
      }

      return {
        success: false,
        error: {
          name: "RegistrationError",
          message: "Failed to create user.",
        },
      };
    },
    forgotPassword: async ({ email }) => {
      console.log(email)
      return {
        success: true,
        redirectTo: "/login",
      };
    },
    updatePassword: async ({ password, token }) => {
      console.log(password, token)
      return {
        success: true,
        redirectTo: "/",
      };
    }
  }
};
