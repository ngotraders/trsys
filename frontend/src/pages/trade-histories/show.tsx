import {
  useShow,
  IResourceComponentsProps,
  useTranslate,
} from "@refinedev/core";
import {
  Show,
  TextFieldComponent as TextField,
  NumberField,
  DateField,
} from "@refinedev/mui";
import { Typography, Stack } from "@mui/material";

export const TradeHistoryShow: React.FC<IResourceComponentsProps> = () => {
  const translate = useTranslate();
  const { queryResult } = useShow();
  const { data, isLoading } = queryResult;

  const record = data?.data;

  return (
    <Show isLoading={isLoading}>
      <Stack gap={1}>
        <Typography variant="body1" fontWeight="bold">
          {translate("trade-histories.fields.ticketNo")}
        </Typography>
        <NumberField value={record?.ticketNo ?? ""} />
        <Typography variant="body1" fontWeight="bold">
          {translate("trade-histories.fields.symbol")}
        </Typography>
        <TextField value={record?.symbol} />
        <Typography variant="body1" fontWeight="bold">
          {translate("trade-histories.fields.orderType")}
        </Typography>
        <NumberField value={record?.orderType ?? ""} />
        <Typography variant="body1" fontWeight="bold">
          {translate("trade-histories.fields.priceOpened")}
        </Typography>
        <NumberField value={record?.priceOpened ?? ""} />
        <Typography variant="body1" fontWeight="bold">
          {translate("trade-histories.fields.timeOpened")}
        </Typography>
        <DateField value={record?.timeOpened} />
        <Typography variant="body1" fontWeight="bold">
          {translate("trade-histories.fields.priceClosed")}
        </Typography>
        <NumberField value={record?.priceClosed ?? ""} />
        <Typography variant="body1" fontWeight="bold">
          {translate("trade-histories.fields.timeClosed")}
        </Typography>
        <DateField value={record?.timeClosed} />
        <Typography variant="body1" fontWeight="bold">
          {translate("trade-histories.fields.percentage")}
        </Typography>
        <NumberField value={record?.percentage ?? ""} />
        <Typography variant="body1" fontWeight="bold">
          {translate("trade-histories.fields.openPublishedAt")}
        </Typography>
        <DateField value={record?.openPublishedAt} />
        <Typography variant="body1" fontWeight="bold">
          {translate("trade-histories.fields.closePublishedAt")}
        </Typography>
        <DateField value={record?.closePublishedAt} />
      </Stack>
    </Show>
  );
};
