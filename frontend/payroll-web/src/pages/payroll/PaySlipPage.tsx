import { useParams } from "react-router-dom";

const PaySlipPage = () => {
  const { id } = useParams();
  return (
    <section>
      <h1>Payslip</h1>
      <p>Payslip details for {id}</p>
    </section>
  );
};

export default PaySlipPage;
