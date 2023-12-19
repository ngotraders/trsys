import {
  useShow,
  IResourceComponentsProps,
  useTranslate,
} from "@refinedev/core";
import {
  Show,
  TextFieldComponent as TextField,
  EmailField,
} from "@refinedev/mui";
import { Typography, Stack } from "@mui/material";

export const UserShow: React.FC<IResourceComponentsProps> = () => {
  const translate = useTranslate();
  const { queryResult } = useShow();
  const { data, isLoading } = queryResult;

  const record = data?.data;

  return (
    <Show isLoading={isLoading}>
      <Stack gap={1}>
        <Typography variant="body1" fontWeight="bold">
          {translate("users.fields.name")}
        </Typography>
        <TextField value={record?.name} />
        <Typography variant="body1" fontWeight="bold">
          {translate("users.fields.username")}
        </Typography>
        <TextField value={record?.username} />
        <Typography variant="body1" fontWeight="bold">
          {translate("users.fields.emailAddress")}
        </Typography>
        <EmailField value={record?.emailAddress} />
        <Typography variant="body1" fontWeight="bold">
          {translate("users.fields.role")}
        </Typography>
        <TextField value={record?.role} />
      </Stack>
    </Show>
  );
};
