import { useEffect, useState } from 'react'
import { passwordGeneratorService } from '@/services/passwordGeneratorService'
import { useDebounce } from '@/hooks/useDebounce'
import type { GeneratePasswordRequest, PasswordStrength } from '@/types'

export function usePasswordGenerator(open: boolean) {
  const [length, setLength] = useState(16)
  const [includeUppercase, setIncludeUppercase] = useState(true)
  const [includeLowercase, setIncludeLowercase] = useState(true)
  const [includeNumbers, setIncludeNumbers] = useState(true)
  const [includeSymbols, setIncludeSymbols] = useState(true)
  const [excludeAmbiguous, setExcludeAmbiguous] = useState(true)
  const [useCharacterDistribution, setUseCharacterDistribution] =
    useState(false)
  const [uppercasePercentage, setUppercasePercentage] = useState(30)
  const [lowercasePercentage, setLowercasePercentage] = useState(45)
  const [numbersPercentage, setNumbersPercentage] = useState(20)
  const [symbolsPercentage, setSymbolsPercentage] = useState(5)
  const [generatedPassword, setGeneratedPassword] = useState('')
  const [passwordStrength, setPasswordStrength] =
    useState<PasswordStrength | null>(null)
  const [isGenerating, setIsGenerating] = useState(false)

  const debouncedLength = useDebounce(length, 500)
  const debouncedUppercasePercentage = useDebounce(uppercasePercentage, 500)
  const debouncedLowercasePercentage = useDebounce(lowercasePercentage, 500)
  const debouncedNumbersPercentage = useDebounce(numbersPercentage, 500)
  const debouncedSymbolsPercentage = useDebounce(symbolsPercentage, 500)

  const generatePassword = async () => {
    setIsGenerating(true)

    try {
      const request: GeneratePasswordRequest = {
        length,
        includeUppercase,
        includeLowercase,
        includeNumbers,
        includeSymbols,
        excludeAmbiguous,
        uppercasePercentage,
        lowercasePercentage,
        numbersPercentage,
        symbolsPercentage,
        useCharacterDistribution,
      }

      const response = await passwordGeneratorService.generate(request)

      if (response.success && response.data) {
        setGeneratedPassword(response.data.password)
        setPasswordStrength(response.data.strength)
      }
    } catch (error) {
      console.error('生成密码失败:', error)
    } finally {
      setIsGenerating(false)
    }
  }

  useEffect(() => {
    if (open) {
      generatePassword()
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open])

  useEffect(() => {
    if (open && generatedPassword) {
      generatePassword()
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    debouncedLength,
    includeUppercase,
    includeLowercase,
    includeNumbers,
    includeSymbols,
    excludeAmbiguous,
    useCharacterDistribution,
    debouncedUppercasePercentage,
    debouncedLowercasePercentage,
    debouncedNumbersPercentage,
    debouncedSymbolsPercentage,
  ])

  return {
    length,
    setLength,
    includeUppercase,
    setIncludeUppercase,
    includeLowercase,
    setIncludeLowercase,
    includeNumbers,
    setIncludeNumbers,
    includeSymbols,
    setIncludeSymbols,
    excludeAmbiguous,
    setExcludeAmbiguous,
    useCharacterDistribution,
    setUseCharacterDistribution,
    uppercasePercentage,
    setUppercasePercentage,
    lowercasePercentage,
    setLowercasePercentage,
    numbersPercentage,
    setNumbersPercentage,
    symbolsPercentage,
    setSymbolsPercentage,
    generatedPassword,
    passwordStrength,
    isGenerating,
    generatePassword,
  }
}