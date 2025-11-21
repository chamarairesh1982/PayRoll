import { ReactNode } from "react";

type DataTableProps = {
  headers: string[];
  rows: ReactNode;
};

const DataTable = ({ headers, rows }: DataTableProps) => (
  <table className="data-table">
    <thead>
      <tr>
        {headers.map((header) => (
          <th key={header}>{header}</th>
        ))}
      </tr>
    </thead>
    <tbody>{rows}</tbody>
  </table>
);

export default DataTable;
