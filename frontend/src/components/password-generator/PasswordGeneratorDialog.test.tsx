import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { PasswordGeneratorDialog } from '@/components/password-generator/PasswordGeneratorDialog'
import * as passwordGeneratorService from '@/services/passwordGeneratorService'

// Mock the password generator service
vi.mock('@/services/passwordGeneratorService', () => ({
  passwordGeneratorService: {
    generate: vi.fn(),
    calculateStrength: vi.fn(),
  },
}))

describe('PasswordGeneratorDialog', () => {
  const mockOnAccept = vi.fn()
  const mockOnOpenChange = vi.fn()

  const mockPasswordResponse = {
    success: true,
    data: {
      password: 'Test1234!@#$',
      strength: {
        score: 75,
        level: 'Strong',
        length: 12,
        hasUppercase: true,
        hasLowercase: true,
        hasNumbers: true,
        hasSymbols: true,
        entropy: 60.5,
      },
    },
    timestamp: new Date().toISOString(),
  }

  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(
      passwordGeneratorService.passwordGeneratorService.generate
    ).mockResolvedValue(mockPasswordResponse)
  })

  it('should render dialog when open', () => {
    render(
      <PasswordGeneratorDialog
        open={true}
        onOpenChange={mockOnOpenChange}
        onAccept={mockOnAccept}
      />
    )

    expect(screen.getByText('密码生成器')).toBeInTheDocument()
  })

  it('should not render dialog when closed', () => {
    render(
      <PasswordGeneratorDialog
        open={false}
        onOpenChange={mockOnOpenChange}
        onAccept={mockOnAccept}
      />
    )

    expect(screen.queryByText('密码生成器')).not.toBeInTheDocument()
  })

  it('should display default password length of 16', () => {
    render(
      <PasswordGeneratorDialog
        open={true}
        onOpenChange={mockOnOpenChange}
        onAccept={mockOnAccept}
      />
    )

    const lengthDisplay = screen.getByText('16')
    expect(lengthDisplay).toBeInTheDocument()
  })

  it('should have all character type options checked by default', async () => {
    render(
      <PasswordGeneratorDialog
        open={true}
        onOpenChange={mockOnOpenChange}
        onAccept={mockOnAccept}
      />
    )

    // Wait for component to render
    await waitFor(() => {
      expect(screen.getByLabelText('大写字母 (A-Z)')).toBeInTheDocument()
    })

    const uppercaseCheckbox = screen.getByLabelText('大写字母 (A-Z)')
    const lowercaseCheckbox = screen.getByLabelText('小写字母 (a-z)')
    const numbersCheckbox = screen.getByLabelText('数字 (0-9)')
    const symbolsCheckbox = screen.getByLabelText('符号 (!@#$%^&*...)')

    expect(uppercaseCheckbox).toBeChecked()
    expect(lowercaseCheckbox).toBeChecked()
    expect(numbersCheckbox).toBeChecked()
    expect(symbolsCheckbox).toBeChecked()
  })

  it('should generate password when clicking generate button', async () => {
    const user = userEvent.setup()

    render(
      <PasswordGeneratorDialog
        open={true}
        onOpenChange={mockOnOpenChange}
        onAccept={mockOnAccept}
      />
    )

    // Wait for initial generation
    await waitFor(() => {
      expect(
        passwordGeneratorService.passwordGeneratorService.generate
      ).toHaveBeenCalledTimes(1)
    })

    const generateButton = screen.getByText('重新生成')
    await user.click(generateButton)

    await waitFor(() => {
      expect(
        passwordGeneratorService.passwordGeneratorService.generate
      ).toHaveBeenCalledTimes(2)
    })
  })

  // Slider interaction testing is complex with Radix UI components
  // We'll skip this test and rely on E2E tests for slider interaction
  it.skip('should update password length when slider changes', async () => {
    // This test is skipped because Radix UI Slider is not a standard input
    // and requires complex interaction testing that's better suited for E2E tests
  })

  it('should toggle character type checkboxes', async () => {
    const user = userEvent.setup()

    render(
      <PasswordGeneratorDialog
        open={true}
        onOpenChange={mockOnOpenChange}
        onAccept={mockOnAccept}
      />
    )

    // Wait for component to render
    await waitFor(() => {
      expect(screen.getByLabelText('符号 (!@#$%^&*...)')).toBeInTheDocument()
    })

    const symbolsCheckbox = screen.getByLabelText('符号 (!@#$%^&*...)')
    expect(symbolsCheckbox).toBeChecked()

    await user.click(symbolsCheckbox)
    expect(symbolsCheckbox).not.toBeChecked()
  })

  it('should call onAccept with generated password when clicking accept button', async () => {
    const user = userEvent.setup()

    render(
      <PasswordGeneratorDialog
        open={true}
        onOpenChange={mockOnOpenChange}
        onAccept={mockOnAccept}
      />
    )

    // Wait for initial password generation
    await waitFor(() => {
      expect(
        passwordGeneratorService.passwordGeneratorService.generate
      ).toHaveBeenCalled()
    })

    const acceptButton = screen.getByText('使用此密码')
    await user.click(acceptButton)

    expect(mockOnAccept).toHaveBeenCalledWith('Test1234!@#$')
    expect(mockOnOpenChange).toHaveBeenCalledWith(false)
  })

  // Dialog can be closed by clicking overlay or pressing ESC
  // But this is hard to test in unit tests, better suited for E2E
  it.skip('should close dialog when clicking cancel button', async () => {
    // This test is skipped because the dialog doesn't have an explicit cancel button
    // It uses the shadcn/ui Dialog component which closes on overlay click or ESC
    // This behavior is better tested in E2E tests
  })

  it('should display generated password', async () => {
    render(
      <PasswordGeneratorDialog
        open={true}
        onOpenChange={mockOnOpenChange}
        onAccept={mockOnAccept}
      />
    )

    // Wait for password generation
    await waitFor(() => {
      expect(screen.getByText('Test1234!@#$')).toBeInTheDocument()
    })
  })

  it('should display password strength information', async () => {
    render(
      <PasswordGeneratorDialog
        open={true}
        onOpenChange={mockOnOpenChange}
        onAccept={mockOnAccept}
      />
    )

    // Wait for password generation and strength display
    await waitFor(() => {
      expect(screen.getByText('强')).toBeInTheDocument()
    })

    // Check for score (text may be split across elements)
    expect(screen.getByText(/75/)).toBeInTheDocument()
    expect(screen.getByText(/分数/)).toBeInTheDocument()
  })

  it('should regenerate password when configuration changes', async () => {
    const user = userEvent.setup()

    render(
      <PasswordGeneratorDialog
        open={true}
        onOpenChange={mockOnOpenChange}
        onAccept={mockOnAccept}
      />
    )

    // Wait for initial generation
    await waitFor(() => {
      expect(
        passwordGeneratorService.passwordGeneratorService.generate
      ).toHaveBeenCalledTimes(1)
    })

    // Wait for component to render
    await waitFor(() => {
      expect(screen.getByLabelText('符号 (!@#$%^&*...)')).toBeInTheDocument()
    })

    // Change configuration
    const symbolsCheckbox = screen.getByLabelText('符号 (!@#$%^&*...)')
    await user.click(symbolsCheckbox)

    // Should trigger regeneration
    await waitFor(() => {
      expect(
        passwordGeneratorService.passwordGeneratorService.generate
      ).toHaveBeenCalledTimes(2)
    })
  })

  it('should handle API errors gracefully', async () => {
    vi.mocked(
      passwordGeneratorService.passwordGeneratorService.generate
    ).mockResolvedValueOnce({
      success: false,
      error: {
        message: 'API Error',
        code: 'GENERATION_ERROR',
      },
    } as any)

    render(
      <PasswordGeneratorDialog
        open={true}
        onOpenChange={mockOnOpenChange}
        onAccept={mockOnAccept}
      />
    )

    // Component should handle error gracefully - password should remain empty
    await waitFor(() => {
      expect(
        passwordGeneratorService.passwordGeneratorService.generate
      ).toHaveBeenCalled()
    })

    // Should still show the empty state
    expect(screen.getByText('...')).toBeInTheDocument()
  })

  it('should prevent accepting password when generation fails', async () => {
    vi.mocked(
      passwordGeneratorService.passwordGeneratorService.generate
    ).mockResolvedValueOnce({
      success: false,
      error: {
        message: 'API Error',
        code: 'GENERATION_ERROR',
      },
    } as any)

    render(
      <PasswordGeneratorDialog
        open={true}
        onOpenChange={mockOnOpenChange}
        onAccept={mockOnAccept}
      />
    )

    await waitFor(() => {
      expect(
        passwordGeneratorService.passwordGeneratorService.generate
      ).toHaveBeenCalled()
    })

    const acceptButton = screen.getByText('使用此密码')
    expect(acceptButton).toBeDisabled()
  })
})
