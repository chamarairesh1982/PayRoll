#!/usr/bin/env python3
"""
Sri Lanka Payroll Test Data Generator (200 Employees)

- 20 mid-month joiners (10th–25th)
- 10 mid-month leavers (5th–20th)
- 30 overtime-heavy (20–50 hours)
- 20 no-pay leave (1–10 days)
- 20 loan repayments
- 15 invalid/negative cases
- 10 tax edge cases
- 75 normal employees
"""

import csv
import json
import os
import random
from datetime import date

random.seed(12345)  # deterministic

SL_BANKS = [
    "Commercial Bank", "Bank of Ceylon", "People's Bank",
    "Sampath Bank", "Hatton National Bank", "DFCC Bank"
]

SL_DEPARTMENTS = ["IT", "Finance", "HR", "Operations", "Sales", "Admin"]

SL_DESIGNATIONS = {
    "IT": ["Software Engineer", "Senior Developer", "Tech Lead", "QA Engineer"],
    "Finance": ["Accountant", "Finance Manager", "Accounts Executive"],
    "HR": ["HR Manager", "HR Executive", "Recruiter"],
    "Operations": ["Operations Manager", "Supervisor", "Coordinator"],
    "Sales": ["Sales Executive", "Sales Manager", "Business Development"],
    "Admin": ["Admin Officer", "Office Manager", "Receptionist"]
}

FIRST_NAMES = [
    "Kasun", "Nimal", "Chamara", "Dilshan", "Sahan",
    "Tharindu", "Dinuka", "Shehan", "Ruwan", "Ishara",
    "Shalini", "Nethmi", "Sachini", "Dilini", "Thilini",
    "Kaushalya", "Hasini", "Lakshi", "Sanduni", "Imesha"
]

LAST_NAMES = [
    "Perera", "Silva", "Fernando", "Jayasinghe", "Gunasekara",
    "Wijesinghe", "Bandara", "Herath", "Rathnayake", "Dissanayake",
    "Wickramasinghe", "Amarasinghe", "Kariyawasam", "Samaraweera", "Alwis",
    "De Zoysa", "Peiris", "Seneviratne", "Ilangakoon", "Pathirana"
]

CITIES = ["Colombo", "Kandy", "Galle", "Kurunegala", "Negombo",
          "Matara", "Jaffna", "Anuradhapura", "Gampaha", "Kalutara"]


def generate_nic() -> str:
    # old: YYDDDSSSSV, new: YYYYDDDSSSSS (simple valid-ish format)
    if random.choice([True, False]):
        year = random.randint(50, 99)
        days = random.randint(1, 366)
        serial = random.randint(1, 9999)
        return f"{year}{days:03d}{serial:04d}V"
    year = random.randint(1950, 2005)
    days = random.randint(1, 366)
    serial = random.randint(1, 99999)
    return f"{year}{days:03d}{serial:05d}"


def generate_epf_number() -> str:
    return f"{random.randint(1000000, 9999999)}"


def generate_bank_account() -> str:
    length = random.choice([15, 16, 17, 18])
    return "".join(str(random.randint(0, 9)) for _ in range(length))


def generate_mobile() -> str:
    prefix = random.choice(["70", "71", "72", "75", "76", "77", "78"])
    return f"+94{prefix}{random.randint(0, 9999999):07d}"


def calculate_salary_components(basic_salary: int) -> dict:
    transport = random.choice([5000, 7500, 10000, 15000])
    mobile = random.choice([2000, 3000, 5000])
    meal = random.choice([3000, 5000, 7500])
    gross = basic_salary + transport + mobile + meal
    return {
        "basic_salary": basic_salary,
        "transport_allowance": transport,
        "mobile_allowance": mobile,
        "meal_allowance": meal,
        "gross_salary": gross
    }


def rand_iso_date(year_from: int, year_to: int) -> str:
    y = random.randint(year_from, year_to)
    m = random.randint(1, 12)
    d = random.randint(1, 28)
    return date(y, m, d).isoformat()


def generate_employee(index: int, scenario: str, invalid_variant: str | None = None) -> dict:
    # salary distribution
    if index < 60:
        basic_salary = random.randint(30000, 50000)
    elif index < 140:
        basic_salary = random.randint(50000, 100000)
    elif index < 180:
        basic_salary = random.randint(100000, 200000)
    else:
        basic_salary = random.randint(200000, 500000)

    department = random.choice(SL_DEPARTMENTS)
    designation = random.choice(SL_DESIGNATIONS[department])

    first = random.choice(FIRST_NAMES)
    last = random.choice(LAST_NAMES)

    employee = {
        "employee_id": f"EMP{index + 1:04d}",
        "first_name": first,
        "last_name": last,
        "nic": generate_nic(),
        "date_of_birth": rand_iso_date(1965, 2003),
        "gender": random.choice(["Male", "Female"]),
        "email": f"emp{index + 1:04d}@company.lk",
        "mobile": generate_mobile(),
        "address": f"{random.randint(10, 250)}, {random.choice(CITIES)}, Sri Lanka",
        "department": department,
        "designation": designation,
        "employment_type": random.choice(["Permanent", "Contract", "Temporary"]),
        "bank_name": random.choice(SL_BANKS),
        "bank_branch": random.choice(CITIES),
        "epf_number": generate_epf_number(),
        "tax_id": generate_nic(),
        "marital_status": random.choice(["Single", "Married"]),
        "dependents": random.randint(0, 3),
        "scenario": scenario
    }

    # scenario rules
    if scenario == "mid_month_joiner":
        join_day = random.randint(10, 25)
        employee["join_date"] = f"2025-01-{join_day:02d}"
        employee["proration_required"] = True

    elif scenario == "mid_month_leaver":
        employee["join_date"] = "2024-06-01"
        leave_day = random.randint(5, 20)
        employee["leave_date"] = f"2025-01-{leave_day:02d}"
        employee["proration_required"] = True
        employee["status"] = "Resigned"

    elif scenario == "overtime_heavy":
        employee["join_date"] = "2023-01-01"
        employee["overtime_hours"] = random.randint(20, 50)
        employee["overtime_type"] = random.choice(["Normal", "Weekend", "Holiday"])

    elif scenario == "no_pay_leave":
        employee["join_date"] = "2023-01-01"
        employee["no_pay_days"] = random.randint(1, 10)

    elif scenario == "loan_repayment":
        employee["join_date"] = "2022-01-01"
        employee["loan_amount"] = random.randint(50000, 500000)
        employee["loan_monthly_deduction"] = random.randint(5000, 25000)

    elif scenario == "tax_edge_case":
        # monthly boundaries (example)
        edge_salaries = [41667, 83333, 125000, 166667]
        basic_salary = random.choice(edge_salaries)
        employee["tax_edge_case"] = True
        employee["join_date"] = rand_iso_date(2020, 2024)

    elif scenario == "invalid_data":
        employee["join_date"] = rand_iso_date(2020, 2024)
        employee["validation_error"] = invalid_variant or "unknown"

        if employee["validation_error"] == "missing_bank":
            employee["bank_account_number"] = ""  # missing
        elif employee["validation_error"] == "invalid_nic":
            employee["nic"] = "INVALID123"
        elif employee["validation_error"] == "missing_epf":
            employee["epf_number"] = ""
        elif employee["validation_error"] == "invalid_email":
            employee["email"] = "invalid-email"
        elif employee["validation_error"] == "negative_ot":
            employee["overtime_hours"] = -5
        elif employee["validation_error"] == "negative_salary":
            basic_salary = -10000  # will flow into salary calc

    else:
        employee["join_date"] = rand_iso_date(2020, 2024)

    # salary + bank account
    employee.update(calculate_salary_components(basic_salary))

    if "bank_account_number" not in employee:
        employee["bank_account_number"] = generate_bank_account()

    return employee


def ensure_dir(path: str) -> None:
    os.makedirs(path, exist_ok=True)


def save_json(employees: list[dict], path: str) -> None:
    with open(path, "w", encoding="utf-8") as f:
        json.dump(employees, f, indent=2, ensure_ascii=False)
    print(f"✅ JSON saved: {path}")


def save_csv(employees, path):
    if not employees:
        return

    # Collect union of all keys (ensures consistent CSV columns)
    all_fields = set()
    for emp in employees:
        all_fields.update(emp.keys())

    # Stable ordering: put common columns first, then the rest sorted
    preferred_order = [
        "employee_id", "first_name", "last_name", "nic", "date_of_birth", "gender",
        "email", "mobile", "address", "department", "designation", "employment_type",
        "join_date", "leave_date", "status",
        "bank_name", "bank_branch", "bank_account_number",
        "epf_number", "tax_id", "marital_status", "dependents",
        "basic_salary", "transport_allowance", "mobile_allowance", "meal_allowance", "gross_salary",
        "overtime_hours", "overtime_type",
        "no_pay_days",
        "loan_amount", "loan_monthly_deduction",
        "proration_required",
        "tax_edge_case",
        "validation_error",
        "scenario"
    ]

    fieldnames = []
    for f in preferred_order:
        if f in all_fields:
            fieldnames.append(f)
            all_fields.remove(f)

    # Add any remaining fields in sorted order (future-proof)
    fieldnames.extend(sorted(all_fields))

    with open(path, "w", newline="", encoding="utf-8") as f:
        w = csv.DictWriter(f, fieldnames=fieldnames, extrasaction="ignore")
        w.writeheader()
        for emp in employees:
            # Ensure missing keys become empty cells
            row = {k: emp.get(k, "") for k in fieldnames}
            w.writerow(row)

    print(f"✅ CSV saved: {path}")



def generate_all() -> list[dict]:
    employees: list[dict] = []
    idx = 0

    plan = [
        ("mid_month_joiner", 20),
        ("mid_month_leaver", 10),
        ("overtime_heavy", 30),
        ("no_pay_leave", 20),
        ("loan_repayment", 20),
        ("tax_edge_case", 10),
        ("normal", 75),
    ]

    # invalid bundle: 15 total (includes 10 missing bank details)
    invalid_variants = (
        ["missing_bank"] * 10 +
        ["invalid_nic", "missing_epf", "invalid_email", "negative_ot", "negative_salary"]
    )

    for v in invalid_variants:
        employees.append(generate_employee(idx, "invalid_data", invalid_variant=v))
        idx += 1

    for scenario, count in plan:
        for _ in range(count):
            employees.append(generate_employee(idx, scenario))
            idx += 1

    if len(employees) != 200:
        raise ValueError(f"Expected 200 employees, got {len(employees)}")

    return employees


def print_summary(employees: list[dict]) -> None:
    from collections import Counter

    print("\n📊 TEST DATA SUMMARY")
    print("=" * 50)
    print(f"Total Employees: {len(employees)}")

    c = Counter(e["scenario"] for e in employees)
    print("\n📋 Scenario Distribution:")
    for k, v in sorted(c.items()):
        print(f"  - {k}: {v}")

    invalids = [e.get("validation_error") for e in employees if e["scenario"] == "invalid_data"]
    if invalids:
        ic = Counter(invalids)
        print("\n⚠️ Invalid Breakdown:")
        for k, v in sorted(ic.items()):
            print(f"  - {k}: {v}")

    salaries = [e["gross_salary"] for e in employees]
    print("\n💰 Salary Range (Gross):")
    print(f"  - Min: LKR {min(salaries):,}")
    print(f"  - Max: LKR {max(salaries):,}")
    print(f"  - Avg: LKR {sum(salaries) // len(salaries):,}")


if __name__ == "__main__":
    print("🚀 Generating Sri Lanka Payroll Test Data (200 employees)...")

    root = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
    fixtures = os.path.join(root, "fixtures")
    ensure_dir(fixtures)

    employees = generate_all()

    json_path = os.path.join(fixtures, "test_data_employees.json")
    csv_path = os.path.join(fixtures, "test_data_employees.csv")

    save_json(employees, json_path)
    save_csv(employees, csv_path)
    print_summary(employees)

    print("\n✅ Done. Files are in fixtures/")
