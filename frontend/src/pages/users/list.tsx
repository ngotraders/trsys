import React from "react";
import {
  useDataGrid,
  EditButton,
  ShowButton,
  DeleteButton,
  List,
  EmailField,
} from "@refinedev/mui";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import { IResourceComponentsProps, useTranslate } from "@refinedev/core";

export const UserList: React.FC<IResourceComponentsProps> = () => {
  const translate = useTranslate();
  const { dataGridProps } = useDataGrid();

  const columns = React.useMemo<GridColDef[]>(
    () => [
      {
        field: "name",
        flex: 1,
        minWidth: 200,
        headerName: translate("users.fields.name"),
        filterable: false,
      },
      {
        field: "username",
        flex: 1,
        minWidth: 200,
        headerName: translate("users.fields.username"),
        filterable: false,
      },
      {
        field: "emailAddress",
        flex: 1,
        minWidth: 250,
        headerName: translate("users.fields.emailAddress"),
        filterable: false,
        renderCell: function render({ value }) {
          return <EmailField value={value} />;
        },
      },
      {
        field: "role",
        flex: 1,
        minWidth: 200,
        headerName: translate("users.fields.role"),
        filterable: false,
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
              <DeleteButton hideText recordItemId={row.id} />
            </>
          );
        },
        align: "center",
        headerAlign: "center",
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
