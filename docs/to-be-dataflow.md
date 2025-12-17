# To-Be Payroll Data Flow Diagrams

## To-Be DFD (Level 0)
```mermaid
graph TD
    HR[Admin/HR]
    Emp[Employee]
    Bank[Bank]
    IRD[IRD]
    EPF[EPF/ETF]
    ACC[Accounting]
    Audit[Audit/Auditor]

    subgraph PayrollSystem
      P1[Employee & Recurring Item Setup]
      P2[Attendance & Leave Capture]
      P3[Payroll Run]
      P4[Approval & Audit]
      P5[Outputs & Exports]
    end

    HR --> P1
    Emp --> P1
    Emp --> P2
    P1 -->|Employees & Recurring Items| D1[(Employees / Recurring Pay Items)]
    P2 -->|Attendance & Leave| D2[(Attendance / Leave)]
    P3 -->|PayRuns & Payslips| D3[(PayRuns / PaySlips)]
    P3 -->|Rules & Rates| D4[(Payroll Rules / Statutory Tables)]
    P4 -->|Approval Trail| D5[(Audit Logs)]
    P5 -->|GL Batches| D6[(GL Postings)]

    D1 --> P3
    D2 --> P3
    D4 --> P3
    P3 --> P4
    P4 --> P3
    P3 --> P5

    P5 -->|Payslips| Emp
    P5 -->|Statutory Schedules| EPF
    P5 -->|PAYE Schedule| IRD
    P5 -->|Bank Transfer File| Bank
    P5 -->|GL Summary| ACC
    P4 --> Audit
```

## To-Be DFD (Level 1 – Payroll Run)
```mermaid
graph TD
    HR[Admin/HR]
    Emp[Employee]

    subgraph DataStores
      D1[(Employee Master & Recurring Items)]
      D2[(Attendance & OT)]
      D3[(Leave Adjustments)]
      D4[(Loans / Advances)]
      D5[(Payroll Rules & Statutory Tables)]
      D6[(PayRuns)]
      D7[(PaySlips)]
      D8[(Audit Trail)]
      D9[(GL Posting Staging)]
    end

    HR -->|Initiate pay period| PR1[Select employees & period]
    PR1 -->|Load master| D1
    PR1 --> PR2[Load attendance & OT]
    D2 --> PR2
    PR1 --> PR3[Load leave adjustments]
    D3 --> PR3
    PR1 --> PR4[Load loans & advances]
    D4 --> PR4

    PR2 --> PR5[Build earnings & deductions]
    PR3 --> PR5
    PR4 --> PR6[Apply one-offs (loans/advances)]
    PR5 --> PR7[Statutory EPF/ETF calculation]
    PR5 --> PR8[PAYE calculation using slabs]
    D5 --> PR7
    D5 --> PR8

    PR7 --> PR9[Assemble payroll results]
    PR8 --> PR9
    PR6 --> PR9
    PR9 -->|Draft payslips| D7
    PR9 -->|Draft pay run| D6

    HR -->|Review| PR10[Review → Approve → Lock]
    PR10 --> D6
    PR10 --> D8
    PR9 --> PR10

    PR10 --> PR11[Generate outputs]
    PR11 -->|Payslips| Emp
    PR11 -->|Statutory Schedules (EPF/ETF)| EPF[EPF/ETF]
    PR11 -->|PAYE Schedule| IRD[IRD]
    PR11 -->|Bank export file| Bank[Bank]
    PR11 -->|GL summary/posting| D9
    D9 --> ACC[Accounting]
```
