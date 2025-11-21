import { useState } from "react";

type Props = {
  onFilter: (period: string) => void;
};

const PayRunFilterForm = ({ onFilter }: Props) => {
  const [period, setPeriod] = useState("");

  return (
    <form
      onSubmit={(event) => {
        event.preventDefault();
        onFilter(period);
      }}
    >
      <label>
        Period
        <input value={period} onChange={(event) => setPeriod(event.target.value)} />
      </label>
      <button type="submit">Filter</button>
    </form>
  );
};

export default PayRunFilterForm;
