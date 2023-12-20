import React from "react";
import {
  useDataGrid,
  EditButton,
  ShowButton,
  DeleteButton,
  List,
  DateField,
} from "@refinedev/mui";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import { IResourceComponentsProps, useTranslate } from "@refinedev/core";
import { Typography } from "@mui/material";

export const TradeHistoryList: React.FC<IResourceComponentsProps> = () => {
  const translate = useTranslate();
  const { dataGridProps } = useDataGrid();

  const columns = React.useMemo<GridColDef[]>(
    () => [
      {
        field: "isOpen",
        flex: 1,
        headerName: translate("trade-histories.fields.isOpen"),
        minWidth: 100,
        renderCell: function render({ value }) {
          return (
            <Typography
              variant="body1"
              fontWeight="bold"
              color={value ? "success.main" : "grey.500"}
            >
              {value ? "OPEN" : "CLOSED"}
            </Typography>
          );
        },
      },
      {
        field: "openPublishedAt",
        flex: 1,
        headerName: translate("trade-histories.fields.openPublishedAt"),
        minWidth: 200,
        renderCell: function render({ value }) {
          return <DateField value={value} format="YYYY-MM-DD HH:mm:ss" />;
        },
      },
      {
        field: "closePublishedAt",
        flex: 1,
        headerName: translate("trade-histories.fields.closePublishedAt"),
        minWidth: 200,
        renderCell: function render({ value }) {
          return value && <DateField value={value} format="YYYY-MM-DD HH:mm:ss" />;
        },
      },
      {
        field: "symbol",
        flex: 1,
        headerName: translate("trade-histories.fields.symbol"),
        minWidth: 120,
      },
      {
        field: "orderType",
        flex: 1,
        headerName: translate("trade-histories.fields.orderType"),
        minWidth: 120,
        renderCell: function render({ value }) {
          return translate(`trade-histories.orderTypes.${value}`);
        },
      },
      {
        field: "ticketNo",
        flex: 1,
        headerName: translate("trade-histories.fields.ticketNo"),
        type: "number",
        minWidth: 120,
      },
      {
        field: "actions",
        headerName: translate("table.actions"),
        sortable: false,
        renderCell: function render({ row }) {
          return (
            <>
              <EditButton hideText recordItemId={row.id} />
              <ShowButton hideText recordItemId={row.id} />
              <DeleteButton hideText recordItemId={row.id} />
            </>
          );
        },
        align: "center",
        headerAlign: "center",
        minWidth: 80,
      },
    ],
    [translate]
  );

  return (
    <List>
      <DataGrid {...dataGridProps} columns={columns} autoHeight />
    </List>
  );
};
