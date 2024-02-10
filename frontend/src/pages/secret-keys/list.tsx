import React from "react";
import {
  useDataGrid,
  EditButton,
  ShowButton,
  DeleteButton,
  List,
} from "@refinedev/mui";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import {
  IResourceComponentsProps,
  useTranslate,
  useUpdate,
} from "@refinedev/core";
import { Checkbox } from "@mui/material";
import SyncIcon from "@mui/icons-material/Sync";

export const SecretKeyList: React.FC<IResourceComponentsProps> = () => {
  const translate = useTranslate();
  const { dataGridProps } = useDataGrid({
    syncWithLocation: true,
  });
  const { mutate } = useUpdate();

  const columns = React.useMemo<GridColDef[]>(
    () => [
      {
        field: "keyType",
        flex: 1,
        minWidth: 100,
        headerName: translate("secret-keys.fields.keyType"),
        filterable: false,
        renderCell: function render({ value }) {
          return <>{translate(`secret-keys.keyTypes.${value}`)}</>;
        },
      },
      {
        field: "key",
        flex: 1,
        minWidth: 200,
        headerName: translate("secret-keys.fields.key"),
        filterable: false,
      },
      {
        field: "description",
        flex: 1,
        minWidth: 200,
        headerName: translate("secret-keys.fields.description"),
        filterable: false,
      },
      {
        field: "isApproved",
        minWidth: 100,
        headerName: translate("secret-keys.fields.isApproved"),
        filterable: false,
        renderCell: function render({ value, row }) {
          return (
            <Checkbox
              checked={!!value}
              onClick={() =>
                mutate({
                  resource: "secret-keys",
                  id: row.id,
                  values: { ...row, isApproved: !value },
                })
              }
            />
          );
        },
      },
      {
        field: "isConnected",
        minWidth: 100,
        headerName: translate("secret-keys.fields.isConnected"),
        filterable: false,
        renderCell: function render({ value }) {
          if (value) {
            return <SyncIcon color="success" />;
          }
          return <SyncIcon color="disabled" />;
        },
      },
      {
        field: "actions",
        minWidth: 80,
        headerName: translate("table.actions"),
        sortable: false,
        filterable: false,
        renderCell: function render({ row }) {
          return (
            <>
              <EditButton hideText recordItemId={row.id} />
              <ShowButton hideText recordItemId={row.id} />
              <DeleteButton
                hideText
                recordItemId={row.id}
                disabled={row.isApproved}
              />
            </>
          );
        },
        align: "center",
        headerAlign: "center",
      },
    ],
    [mutate, translate]
  );

  return (
    <List>
      <DataGrid {...dataGridProps} columns={columns} autoHeight />
    </List>
  );
};
