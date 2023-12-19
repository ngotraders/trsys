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
        field: "publisherId",
        flex: 1,
        headerName: translate("trade-histories.fields.publisherId"),
        minWidth: 300,
      },
      {
        field: "ticketNo",
        flex: 1,
        headerName: translate("trade-histories.fields.ticketNo"),
        type: "number",
        minWidth: 150,
      },
      {
        field: "symbol",
        flex: 1,
        headerName: translate("trade-histories.fields.symbol"),
        minWidth: 150,
      },
      {
        field: "orderType",
        flex: 1,
        headerName: translate("trade-histories.fields.orderType"),
        minWidth: 150,
        renderCell: function render({ value }) {
          return translate(`trade-histories.orderTypes.${value}`);
        },
      },
      {
        field: "openPublishedAt",
        flex: 1,
        headerName: translate("trade-histories.fields.openPublishedAt"),
        minWidth: 250,
        renderCell: function render({ value }) {
          return <DateField value={value} format="YYYY-MM-DD HH:mm:ss" />;
        },
      },
      {
        field: "closePublishedAt",
        flex: 1,
        headerName: translate("trade-histories.fields.closePublishedAt"),
        minWidth: 250,
        renderCell: function render({ value }) {
          return <DateField value={value} format="YYYY-MM-DD HH:mm:ss" />;
        },
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
