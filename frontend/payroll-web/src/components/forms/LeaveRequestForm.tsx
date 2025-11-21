import { useState } from "react";
import { LeaveRequest } from "../../types/Leave";

type Props = {
  onSubmit: (leave: Partial<LeaveRequest>) => void;
};

const LeaveRequestForm = ({ onSubmit }: Props) => {
  const [reason, setReason] = useState("");

  return (
    <form
      onSubmit={(event) => {
        event.preventDefault();
        onSubmit({ reason });
      }}
    >
      <label>
        Reason
        <input value={reason} onChange={(event) => setReason(event.target.value)} />
      </label>
      <button type="submit">Submit</button>
    </form>
  );
};

export default LeaveRequestForm;
