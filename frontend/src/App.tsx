import { Authenticated, Refine } from "@refinedev/core";
import { DevtoolsPanel, DevtoolsProvider } from "@refinedev/devtools";
import { RefineKbar, RefineKbarProvider } from "@refinedev/kbar";
import axios, { AxiosInstance } from "axios";

import {
  ErrorComponent,
  notificationProvider,
  RefineSnackbarProvider,
  ThemedLayoutV2,
} from "@refinedev/mui";

import CssBaseline from "@mui/material/CssBaseline";
import GlobalStyles from "@mui/material/GlobalStyles";
import PeopleIcon from "@mui/icons-material/People";
import KeyIcon from "@mui/icons-material/Key";
import ShowChartIcon from "@mui/icons-material/ShowChart";
import ReceiptIcon from '@mui/icons-material/Receipt'
import routerBindings, {
  CatchAllNavigate,
  DocumentTitleHandler,
  NavigateToResource,
  UnsavedChangesNotifier,
} from "@refinedev/react-router-v6";
import dataProvider from "@refinedev/simple-rest";
import { useTranslation } from "react-i18next";
import { BrowserRouter, Outlet, Route, Routes } from "react-router-dom";
import { authProvider } from "./authProvider";
import { Header } from "./components/header";
import { ColorModeContextProvider } from "./contexts/color-mode";
import { UserCreate, UserEdit, UserList, UserShow } from "./pages/users";
import {
  SecretKeyCreate,
  SecretKeyEdit,
  SecretKeyList,
  SecretKeyShow,
} from "./pages/secret-keys";
import {
  TradeHistoryCreate,
  TradeHistoryEdit,
  TradeHistoryList,
  TradeHistoryShow,
} from "./pages/trade-histories";
import {
  EventCreate,
  EventEdit,
  EventList,
  EventShow,
} from "./pages/events";
import { ForgotPassword } from "./pages/forgotPassword";
import { Login } from "./pages/login";
import { Register } from "./pages/register";
import { UpdatePassword } from "./pages/updatePassword";
import { Logo } from "./components/logo";

const axiosInstance: AxiosInstance = axios.create({
  withCredentials: true,
});

function App() {
  const { t, i18n } = useTranslation();

  const i18nProvider = {
    translate: (key: string, params: object) => t(key, params),
    changeLocale: (lang: string) => i18n.changeLanguage(lang),
    getLocale: () => i18n.language,
  };

  return (
    <BrowserRouter>
      <RefineKbarProvider>
        <ColorModeContextProvider>
          <CssBaseline />
          <GlobalStyles styles={{ html: { WebkitFontSmoothing: "auto" } }} />
          <RefineSnackbarProvider>
            <DevtoolsProvider>
              <Refine
                dataProvider={{
                  default: dataProvider("https://api.fake-rest.refine.dev"),
                  trsys: dataProvider(
                    "/api/admin",
                    // eslint-disable-next-line @typescript-eslint/no-explicit-any
                    axiosInstance as unknown as any
                  ),
                }}
                notificationProvider={notificationProvider}
                routerProvider={routerBindings}
                authProvider={authProvider("/auth", axiosInstance)}
                i18nProvider={i18nProvider}
                resources={[
                  {
                    name: "users",
                    list: "/users",
                    create: "/users/create",
                    edit: "/users/edit/:id",
                    show: "/users/show/:id",
                    meta: {
                      dataProviderName: "trsys",
                      canDelete: true,
                      title: "ユーザー",
                      label: "ユーザー",
                      icon: <PeopleIcon />,
                    },
                  },
                  {
                    name: "secret-keys",
                    list: "/secret-keys",
                    create: "/secret-keys/create",
                    edit: "/secret-keys/edit/:id",
                    show: "/secret-keys/show/:id",
                    meta: {
                      dataProviderName: "trsys",
                      canDelete: true,
                      title: "シークレットキー",
                      label: "シークレットキー",
                      icon: <KeyIcon />,
                    },
                  },
                  {
                    name: "trade-histories",
                    list: "/trade-histories",
                    create: "/trade-histories/create",
                    edit: "/trade-histories/edit/:id",
                    show: "/trade-histories/show/:id",
                    meta: {
                      dataProviderName: "trsys",
                      canDelete: true,
                      title: "トレード",
                      label: "トレード",
                      icon: <ShowChartIcon />,
                    },
                  },
                  {
                    name: "events",
                    list: "/events",
                    create: "/events/create",
                    edit: "/events/edit/:id",
                    show: "/events/show/:id",
                    meta: {
                      dataProviderName: "trsys",
                      canDelete: true,
                      title: "イベント",
                      label: "イベント",
                      icon: <ReceiptIcon />,
                    },
                  },
                ]}
                options={{
                  syncWithLocation: true,
                  warnWhenUnsavedChanges: false,
                  useNewQueryKeys: true,
                  projectId: "5CiY82-FEStUw-1Msx23",
                }}
              >
                <Routes>
                  <Route
                    element={
                      <Authenticated
                        key="authenticated-inner"
                        fallback={<CatchAllNavigate to="/login" />}
                      >
                        <ThemedLayoutV2
                          Title={(props) =>
                            props.collapsed ? null : <Logo size="sm" />
                          }
                          Header={() => <Header sticky />}
                        >
                          <Outlet />
                        </ThemedLayoutV2>
                      </Authenticated>
                    }
                  >
                    <Route
                      index
                      element={<NavigateToResource resource="users" />}
                    />
                    <Route path="/users">
                      <Route index element={<UserList />} />
                      <Route path="create" element={<UserCreate />} />
                      <Route path="edit/:id" element={<UserEdit />} />
                      <Route path="show/:id" element={<UserShow />} />
                    </Route>
                    <Route path="/secret-keys">
                      <Route index element={<SecretKeyList />} />
                      <Route path="create" element={<SecretKeyCreate />} />
                      <Route path="edit/:id" element={<SecretKeyEdit />} />
                      <Route path="show/:id" element={<SecretKeyShow />} />
                    </Route>
                    <Route path="/trade-histories">
                      <Route index element={<TradeHistoryList />} />
                      <Route path="create" element={<TradeHistoryCreate />} />
                      <Route path="edit/:id" element={<TradeHistoryEdit />} />
                      <Route path="show/:id" element={<TradeHistoryShow />} />
                    </Route>
                    <Route path="/events">
                      <Route index element={<EventList />} />
                      <Route path="create" element={<EventCreate />} />
                      <Route path="edit/:id" element={<EventEdit />} />
                      <Route path="show/:id" element={<EventShow />} />
                    </Route>
                    <Route
                      path="/update-password"
                      element={<UpdatePassword />}
                    />
                    <Route path="*" element={<ErrorComponent />} />
                  </Route>
                  <Route
                    element={
                      <Authenticated
                        key="authenticated-outer"
                        fallback={<Outlet />}
                      >
                        <NavigateToResource />
                      </Authenticated>
                    }
                  >
                    <Route path="/login" element={<Login />} />
                    <Route path="/register" element={<Register />} />
                    <Route
                      path="/forgot-password"
                      element={<ForgotPassword />}
                    />
                  </Route>
                </Routes>

                <RefineKbar />
                <UnsavedChangesNotifier />
                <DocumentTitleHandler />
              </Refine>
              <DevtoolsPanel />
            </DevtoolsProvider>
          </RefineSnackbarProvider>
        </ColorModeContextProvider>
      </RefineKbarProvider>
    </BrowserRouter>
  );
}

export default App;
