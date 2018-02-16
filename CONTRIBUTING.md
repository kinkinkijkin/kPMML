English

Compiler:
- Try not to touch the overall architecture without clearance from kinkinkijkin.
- If using parallelism, try to implement it using the Parallel class, not Thread.
- Only split lines after 90 chars per line. Lines longer that 90 chars must be split.
- Modifying the code spec in a way that will make older modules no longer compile
without explicit permission from kinkinkijkin will not be accepted.
- Additions to code spec must be in shorthand forms of english, but comments can be in
english or french.
- Try to prioritize fixing bugs over adding features.
- If adding new features to the code spec, follow the following rules:
    - type names are always capitalized and 3-4 characters
    - input floating-point numbers shoud NOT be doubles or larger,
    only singles or halfs.
    - use T.Parse for getting info from strings over Convert.ToT

Examples:
- If providing examples, ADD COMMENTS. Comment-bare examples will not be accepted.
- Examples should not exist just to sound good, but to show off some feature of the
code spec.
- Example file names should match their metadata or be simply "referenceexamplex.txt"
- "referenceexamplex.txt" is reserved for examples which show off many features and
are relatively unambiguous in their writing.


Français

Compilateur:
- Si vous n'avez pas d'autorization de kinkinkijkin pour modifier l'architecture globale du
compilateur, ne le faites pas.
- Pour l'utiliser du parallélisme, utilisez la classe Parallel, pas du Thread.
- Séparez vos lignes à 90 chars !
- Si vous n'avez pas d'autorization EXPLICITE de kinkinkijkin, ne modifiez pas le code-spéc
de manière à ce que les modules vieux compilerai pas, ne le faites pas.
- Les ajouts au code-spéc doivent être en anglais, mais les remarques peuvent 
être en anglais ou français.
- Les bogues sont plus importants que les améliorations !
- Si vous faites de les améliorations nouvelles du code-spéc:
    - Les noms des types dois être des abréviations anglaises
    - des floats dois être des singles, pas des doubles ou des halfs !
    - utilisez T.Parse pour convertir des strings, pas de Convert.ToT !

Exemples:
- COMMENTAIRE, COMMENTAIRE, COMMENTAIRE ! Vos exemples a besoin des remarques !
- Exemples dois utiliser amplement des fonctions.
- Les noms des fichiers des exemples dois corresprendre à leurs metadatas ou
« referenceexamplex.txt »
- « referenceexamplex.txt » est reservé pour les exemples qui utilisent beaucoup de
fonctions code-spéc, dans un style non ambigû.