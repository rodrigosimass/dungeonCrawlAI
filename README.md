# Como instalar:

- Ter pasta com o Lab5 com as cenas do Visual Studio (a última cena que mandei)
- Apagar pasta dos Assets (este repo já a tem na íntegra)
- Abrir um *Git Bash* na pasta (onde está localizada a pasta Assets)
- Comandos de *git* para o setup:
  - `git init`
  - `git remote add origin https://github.com/Kronostheus/IAJ-proj3.git`
  - `git pull origin master`
- Correr projeto no Unity para certificar que está tudo bem

# Nunca fazer push de código não funcional para o git!

## Se quiserem trabalhar com branches:
**Recomendo usarem um Git GUI Client (ex. SourceTree/GitKraken/etc.) para facilitar merges e afins**

- Certificar que têm código do *master* mais atualizado: `git pull origin master`
- Criar novo local branch: `git checkout -b [branch_name]`
- Certificar que estão no branch certo: `git branch -a` em que o \* vai estar no branch em que estão
  - Para mudar de branch: `git checkout [branch_name]`
- Fazer `push` para o vosso branch: `git push origin [branch_name]`
