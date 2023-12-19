import React from "react";
import {
  useDataGrid,
  EditButton,
  ShowButton,
  DeleteButton,
  List,
} from "@refinedev/mui";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import { IResourceComponentsProps, useTranslate } from "@refinedev/core";
import { Checkbox } from "@mui/material";

export const SecretKeyList: React.FC<IResourceComponentsProps> = () => {
  const translate = useTranslate();
  const { dataGridProps } = useDataGrid();

  const columns = React.useMemo<GridColDef[]>(
    () => [
      {
        field: "keyType",
        flex: 1,
        headerName: translate("secret-keys.fields.keyType"),
        renderCell: function render({ value }) {
          return <>{translate(`secret-keys.keyTypes.${value}`)}</>;
        },
        minWidth: 100,
      },
      {
        field: "key",
        flex: 1,
        headerName: translate("secret-keys.fields.key"),
        minWidth: 200,
      },
      {
        field: "isApproved",
        headerName: translate("secret-keys.fields.isApproved"),
        minWidth: 100,
        renderCell: function render({ value }) {
          return <Checkbox checked={!!value} />;
        },
      },
      {
        field: "isConnected",
        headerName: translate("secret-keys.fields.isConnected"),
        minWidth: 100,
        renderCell: function render({ value }) {
          return <Checkbox checked={!!value} />;
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
