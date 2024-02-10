import React from "react";
import { useDataGrid, List, DateField } from "@refinedev/mui";
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
        minWidth: 100,
        headerName: translate("events.fields.timestamp"),
        filterable: false,
        renderCell: function render({ value }) {
          return <DateField value={value} format="YYYY-MM-DD HH:mm:ss" />;
        },
      },
      {
        field: "aggregateId",
        flex: 1,
        minWidth: 300,
        headerName: translate("events.fields.aggregateId"),
        filterable: false,
      },
      {
        field: "version",
        flex: 1,
        minWidth: 50,
        headerName: translate("events.fields.version"),
        filterable: false,
        type: "number",
      },
      {
        field: "eventType",
        flex: 1,
        minWidth: 200,
        headerName: translate("events.fields.eventType"),
        filterable: false,
      },
      {
        field: "data",
        flex: 1,
        minWidth: 300,
        headerName: translate("events.fields.data"),
        filterable: false,
      },
    ],
    [translate]
  );

  return (
    <List canCreate={false}>
      <DataGrid {...dataGridProps} columns={columns} autoHeight />
    </List>
  );
};
