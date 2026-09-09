using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Route("api/payroll")]
[Authorize]
public class PayrollController : ApiControllerBase
{
    private readonly IPayrollService _service;

    public PayrollController(IPayrollService service) => _service = service;

    // ---------- Salary components ----------

    [Authorize(Policy = "payroll.view")]
    [HttpGet("components")]
    public async Task<IActionResult> GetComponents() =>
        Ok(await _service.GetComponentsAsync(OrgId));

    [Authorize(Policy = "payroll.create")]
    [HttpPost("components")]
    public async Task<IActionResult> CreateComponent([FromBody] CreateSalaryComponentDto dto) =>
        Ok(await _service.CreateComponentAsync(OrgId, dto));

    [Authorize(Policy = "payroll.edit")]
    [HttpPut("components/{id}")]
    public async Task<IActionResult> UpdateComponent(int id, [FromBody] UpdateSalaryComponentDto dto) =>
        Ok(await _service.UpdateComponentAsync(OrgId, id, dto));

    [Authorize(Policy = "payroll.delete")]
    [HttpDelete("components/{id}")]
    public async Task<IActionResult> DeleteComponent(int id)
    {
        await _service.DeleteComponentAsync(OrgId, id);
        return NoContent();
    }

    // ---------- Employee salaries ----------

    [Authorize(Policy = "payroll.view")]
    [HttpGet("salaries/employee/{employeeId}")]
    public async Task<IActionResult> GetEmployeeSalary(int employeeId)
    {
        var salary = await _service.GetLatestSalaryAsync(OrgId, employeeId);
        return salary == null ? NotFound() : Ok(salary);
    }

    [Authorize(Policy = "payroll.view")]
    [HttpGet("salaries/employee/{employeeId}/history")]
    public async Task<IActionResult> GetEmployeeSalaryHistory(int employeeId) =>
        Ok(await _service.GetSalaryHistoryAsync(OrgId, employeeId));

    [Authorize(Policy = "payroll.edit")]
    [HttpPost("salaries")]
    public async Task<IActionResult> SetSalary([FromBody] SetEmployeeSalaryDto dto) =>
        Ok(await _service.SetSalaryAsync(OrgId, dto));

    [HttpGet("salaries/my")]
    public async Task<IActionResult> GetMySalary()
    {
        var employeeId = EmployeeId
            ?? throw new KeyNotFoundException("Current user is not linked to an employee profile.");

        var salary = await _service.GetLatestSalaryAsync(OrgId, employeeId);
        return salary == null ? NotFound() : Ok(salary);
    }

    // ---------- Payroll runs ----------

    [Authorize(Policy = "payroll.view")]
    [HttpGet("runs")]
    public async Task<IActionResult> GetRuns() =>
        Ok(await _service.GetRunsAsync(OrgId));

    [Authorize(Policy = "payroll.view")]
    [HttpGet("runs/{id}")]
    public async Task<IActionResult> GetRun(int id)
    {
        var (run, payslips) = await _service.GetRunDetailAsync(OrgId, id);
        return Ok(new { run, payslips });
    }

    [Authorize(Policy = "payroll.process")]
    [HttpPost("runs")]
    public async Task<IActionResult> CreateRun([FromBody] CreatePayrollRunDto dto)
    {
        var result = await _service.CreateRunAsync(OrgId, dto);
        return CreatedAtAction(nameof(GetRun), new { id = result.Id }, result);
    }

    [Authorize(Policy = "payroll.process")]
    [HttpPost("runs/{id}/process")]
    public async Task<IActionResult> ProcessRun(int id) =>
        Ok(await _service.ProcessRunAsync(OrgId, id, UserId, UserName));

    [Authorize(Policy = "payroll.lock")]
    [HttpPost("runs/{id}/lock")]
    public async Task<IActionResult> LockRun(int id) =>
        Ok(await _service.LockRunAsync(OrgId, id));

    [Authorize(Policy = "payroll.lock")]
    [HttpPost("runs/{id}/mark-paid")]
    public async Task<IActionResult> MarkPaid(int id) =>
        Ok(await _service.MarkPaidAsync(OrgId, id));

    [Authorize(Policy = "payroll.delete")]
    [HttpDelete("runs/{id}")]
    public async Task<IActionResult> DeleteRun(int id)
    {
        await _service.DeleteRunAsync(OrgId, id);
        return NoContent();
    }

    [Authorize(Policy = "payroll.process")]
    [HttpGet("runs/{id}/bank-file")]
    public async Task<IActionResult> GetBankFile(int id)
    {
        var (bytes, year, month) = await _service.GetBankFileAsync(OrgId, id);
        return File(bytes, "text/csv", $"bank-file-{year}-{month}.csv");
    }

    // ---------- Payslips ----------

    [Authorize(Policy = "payroll.view")]
    [HttpGet("payslips/run/{runId}")]
    public async Task<IActionResult> GetPayslipsByRun(int runId) =>
        Ok(await _service.GetPayslipsByRunAsync(OrgId, runId));

    [Authorize(Policy = "payroll.view")]
    [HttpGet("payslips/{id}")]
    public async Task<IActionResult> GetPayslip(int id) =>
        Ok(await _service.GetPayslipAsync(OrgId, id));

    [HttpGet("payslips/my")]
    public async Task<IActionResult> GetMyPayslips()
    {
        var employeeId = EmployeeId
            ?? throw new KeyNotFoundException("Current user is not linked to an employee profile.");

        return Ok(await _service.GetMyPayslipsAsync(OrgId, employeeId));
    }

    [HttpGet("payslips/my/{id}")]
    public async Task<IActionResult> GetMyPayslip(int id)
    {
        var employeeId = EmployeeId
            ?? throw new KeyNotFoundException("Current user is not linked to an employee profile.");

        return Ok(await _service.GetMyPayslipAsync(OrgId, employeeId, id));
    }
}
