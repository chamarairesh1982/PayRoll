import { useParams } from "react-router-dom";

const EmployeeDetailPage = () => {
  const { id } = useParams();
  return (
    <section>
      <h1>Employee detail</h1>
      <p>Viewing employee {id}</p>
    </section>
  );
};

export default EmployeeDetailPage;
