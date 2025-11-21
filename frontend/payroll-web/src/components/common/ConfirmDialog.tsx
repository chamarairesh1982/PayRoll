import { ReactNode } from "react";

type ConfirmDialogProps = {
  title: string;
  children: ReactNode;
  onConfirm: () => void;
  onCancel: () => void;
};

const ConfirmDialog = ({ title, children, onConfirm, onCancel }: ConfirmDialogProps) => (
  <div className="confirm-dialog">
    <h3>{title}</h3>
    <div className="body">{children}</div>
    <div className="actions">
      <button onClick={onCancel}>Cancel</button>
      <button onClick={onConfirm}>Confirm</button>
    </div>
  </div>
);

export default ConfirmDialog;
