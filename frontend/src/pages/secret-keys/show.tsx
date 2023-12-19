import {
  useShow,
  IResourceComponentsProps,
  useTranslate,
} from "@refinedev/core";
import {
  Show,
  TextFieldComponent as TextField,
  BooleanField,
} from "@refinedev/mui";
import { Typography, Stack, Box } from "@mui/material";

export const SecretKeyShow: React.FC<IResourceComponentsProps> = () => {
  const translate = useTranslate();
  const { queryResult } = useShow();
  const { data, isLoading } = queryResult;

  const record = data?.data;

  return (
    <Show isLoading={isLoading} canDelete={!record?.isApproved}>
      <Stack gap={1}>
        <Typography variant="body1" fontWeight="bold">
          {translate("secret-keys.fields.keyType")}
        </Typography>
        <TextField
          value={translate(`secret-keys.keyTypes.${record?.keyType}`)}
        />
        <Typography variant="body1" fontWeight="bold">
          {translate("secret-keys.fields.key")}
        </Typography>
        <TextField value={record?.key} />
        <Typography variant="body1" fontWeight="bold">
          {translate("secret-keys.fields.token")}
        </Typography>
        <TextField value={record?.token} />
        <Box display="flex">
          <Box mr={3}>
            <Typography variant="body1" fontWeight="bold">
              {translate("secret-keys.fields.isApproved")}
            </Typography>
            <BooleanField value={record?.isApproved} />
          </Box>
          <Box>
            <Typography variant="body1" fontWeight="bold">
              {translate("secret-keys.fields.isConnected")}
            </Typography>
            <BooleanField value={record?.isConnected} />
          </Box>
        </Box>
      </Stack>
    </Show>
  );
};
