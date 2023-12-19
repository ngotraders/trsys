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
        field: "description",
        flex: 1,
        headerName: translate("secret-keys.fields.description"),
        minWidth: 200,
      },
      {
        field: "isApproved",
        headerName: translate("secret-keys.fields.isApproved"),
        minWidth: 100,
        renderCell: function render({ value, row }) {
          return (
            <Checkbox
              checked={!!value}
              onClick={async () => {
                await mutate({
                  resource: "secret-keys",
                  id: row.id,
                  values: { ...row, isApproved: !value },
                });
              }}
            />
          );
        },
      },
      {
        field: "isConnected",
        headerName: translate("secret-keys.fields.isConnected"),
        minWidth: 100,
        renderCell: function render({ value }) {
          if (value) {
            return <SyncIcon color="success" />;
          }
          return <SyncIcon color="disabled" />;
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
        minWidth: 80,
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
