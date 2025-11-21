import { useParams } from "react-router-dom";

const PayRunDetailPage = () => {
  const { id } = useParams();
  return (
    <section>
      <h1>Pay run detail</h1>
      <p>Viewing pay run {id}</p>
    </section>
  );
};

export default PayRunDetailPage;
