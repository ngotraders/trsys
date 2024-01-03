import React from "react";
import {
  useDataGrid,
  List,
  DateField,
} from "@refinedev/mui";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import { IResourceComponentsProps, useTranslate } from "@refinedev/core";

export const EventList: React.FC<IResourceComponentsProps> = () => {
  const translate = useTranslate();
  const { dataGridProps } = useDataGrid();

  const columns = React.useMemo<GridColDef[]>(
    () => [
      {
        field: "timestamp",
        flex: 1,
        headerName: translate("events.fields.timestamp"),
        minWidth: 100,
        renderCell: function render({ value }) {
          return <DateField value={value} format="YYYY-MM-DD HH:mm:ss" />;
        },
      },
      {
        field: "aggregateId",
        flex: 1,
        headerName: translate("events.fields.aggregateId"),
        minWidth: 300,
      },
      {
        field: "version",
        flex: 1,
        headerName: translate("events.fields.version"),
        type: "number",
        minWidth: 50,
      },
      {
        field: "eventType",
        flex: 1,
        headerName: translate("events.fields.eventType"),
        minWidth: 200,
      },
      {
        field: "data",
        flex: 1,
        headerName: translate("events.fields.data"),
        minWidth: 300,
      },
    ],
    [translate],
  );

  return (
    <List canCreate={false}>
      <DataGrid {...dataGridProps} columns={columns} autoHeight />
    </List>
  );
};
