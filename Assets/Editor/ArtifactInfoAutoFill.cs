using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Museum.Artifact;

namespace Museum.EditorTools
{
    /// <summary>
    /// One-shot utility: fills displayName / era / description on every ArtifactInfo
    /// in the active scene from a hard-coded dictionary keyed by GameObject name.
    /// Override existing non-empty fields = false by default — re-run is safe.
    /// </summary>
    public static class ArtifactInfoAutoFill
    {
        const string MenuPath = "Tools/Museum/Phase 3/Auto-Fill Artifact Info";
        const string MenuPathOverwrite = "Tools/Museum/Phase 3/Auto-Fill Artifact Info (Overwrite)";

        struct Entry
        {
            public string era;
            public string description;
            public Entry(string era, string description) { this.era = era; this.description = description; }
        }

        static readonly Dictionary<string, Entry> Catalog = new Dictionary<string, Entry>(System.StringComparer.OrdinalIgnoreCase)
        {
            ["Ankh Khonsu"] = new Entry("Late Period, 26th Dynasty (~664–525 BCE)",
                "A black granite block statue of Ankh-Khonsu, an official and priest of the god Khonsu at Karnak. The cubic form lets the priest 'wear' lengthy biographical and religious inscriptions — a hallmark of Late Period elite statuary."),

            ["Apis Bull"] = new Entry("Late Period to Ptolemaic (~664–30 BCE)",
                "A bronze figure of the Apis bull, the living incarnation of Ptah at Memphis. The solar disk between the horns and the triangular forehead marking identify the sacred animal that, when it died, was mummified and entombed in the Serapeum at Saqqara."),

            ["Baboon"] = new Entry("New Kingdom to Late Period",
                "A seated baboon representing Thoth, god of wisdom, writing, and the moon. Thoth was worshipped in baboon and ibis forms; cult centres at Hermopolis kept and mummified thousands of these animals as votive offerings."),

            ["Block Statue"] = new Entry("Middle Kingdom to Late Period",
                "A 'block statue' — a seated official with knees drawn up, body wrapped in a cloak forming a cubic mass. Popularised in the Middle Kingdom, this format gave priests and officials a durable, inscription-friendly votive image they could place in temples."),

            ["Canopic Jar"] = new Entry("New Kingdom to Late Period",
                "A canopic jar — one of a set of four that held the embalmed lungs, liver, stomach and intestines of the deceased. Each lid depicts a Son of Horus: human-headed Imsety, baboon Hapy, jackal Duamutef, and falcon Qebehsenuef."),

            ["Cat Statue"] = new Entry("Late Period (~664–332 BCE)",
                "A bronze seated cat sacred to Bastet, goddess of home, fertility, and protection. Bubastis was her cult centre. Cats were mummified by the hundreds of thousands and offered to her temples."),

            ["Chest of Tutankhamun"] = new Entry("New Kingdom, 18th Dynasty, reign of Tutankhamun (~1336–1326 BCE)",
                "A painted wooden chest from the tomb of Tutankhamun (KV62). Its scenes show the boy-king triumphant in battle and on the lion hunt — ritual images projecting royal power onto his afterlife."),

            ["Coffin of Akhenaten"] = new Entry("New Kingdom, late 18th Dynasty, Amarna Period (~1336 BCE)",
                "Coffin associated with Akhenaten, the heretic pharaoh who moved the capital to Akhetaten and elevated the sun-disk Aten above the traditional gods. After his death his name was struck from monuments — surviving objects from this short period are exceptionally rare."),

            ["Coffin of Amunred"] = new Entry("Third Intermediate Period (~1069–664 BCE)",
                "Brightly painted anthropoid coffin of a Theban priest of Amun. After the New Kingdom, royal tombs were less elaborate; high-status priests instead invested in finely decorated nested coffins covered in protective spells and judgment scenes."),

            ["Coffin of Mut-iy-iy"] = new Entry("Third Intermediate Period (~1069–664 BCE)",
                "Anthropoid coffin of Mut-iy-iy. The painted wig, broad collar, and protective deities along the lid reproduce the funerary papyrus tradition directly onto the coffin's surface — the body itself becomes a Book of the Dead."),

            ["Egyptian Boat"] = new Entry("Middle Kingdom to New Kingdom",
                "A model funerary boat, placed in tombs to ferry the deceased through the netherworld and to journey with Ra across the sky. Larger versions, like Khufu's full-size cedar barque, were buried near royal pyramids."),

            ["Egyptian Khopesh"] = new Entry("New Kingdom (~1550–1070 BCE)",
                "A khopesh — the iconic sickle-sword carried by Egyptian elite soldiers and pharaohs. Its hooked blade was effective against shields and is shown in royal smiting scenes from Thutmose III through Ramses II."),

            ["False Door of Ni-ankh-Snefru"] = new Entry("Old Kingdom, 4th–5th Dynasty (~2600–2400 BCE)",
                "A false-door stela for the ka of Ni-ankh-Snefru. Carved into the west wall of mastaba chapels, it served as the threshold through which the soul could leave the burial chamber to receive offerings made by the living."),

            ["Goddess Sekhmet"] = new Entry("New Kingdom, 18th Dynasty (~1390 BCE)",
                "Granite statue of Sekhmet, lioness goddess of war and plague. Amenhotep III commissioned hundreds for his mortuary temple at Thebes — one for each day of the year, to appease her destructive power."),

            ["Goddess Taweret"] = new Entry("Late Period (~664–332 BCE)",
                "A hippopotamus-headed Taweret, goddess of childbirth and protector of mother and child. Her composite body (lion, hippo, crocodile) made her a fearsome guardian; small amulets of her are among the most common household objects from Egypt."),

            ["Head of Tutankhamun"] = new Entry("New Kingdom, 18th Dynasty (~1332 BCE)",
                "A wooden head of the boy-king Tutankhamun emerging from a lotus blossom, found in his tomb. The image equates the young pharaoh with the rising sun reborn from the primordial waters at the dawn of creation."),

            ["Horus Statue"] = new Entry("Late Period to Ptolemaic",
                "Falcon statue of Horus, sky god and son of Osiris and Isis. Edfu's well-preserved temple is dedicated to him. The pharaoh was conceived as the living Horus, making this image both religious and royal."),

            ["Inner Coffin of Djedmut"] = new Entry("Third Intermediate Period (~1069–664 BCE)",
                "Inner coffin of the lady Djedmut, a Theban priestess. Densely painted with protective deities, scenes from the Amduat, and offering formulas — the inner coffin sat inside one or two outer coffins, each layer adding another shell of magical protection."),

            ["Khafre Enthroned"] = new Entry("Old Kingdom, 4th Dynasty (~2570 BCE)",
                "The diorite seated statue of Khafre, builder of the second Giza pyramid. Horus enfolds the king's head with his wings, fusing earthly and divine kingship in one image. The hard imported stone alone is a statement of state power."),

            ["Khasekhemwy"] = new Entry("Early Dynastic, 2nd Dynasty (~2700 BCE)",
                "Seated statue of Khasekhemwy, last king of the 2nd Dynasty. Inscriptions along the base record the slain enemies of his northern campaign — among the earliest royal sculptures in Egyptian history."),

            ["King Djoser"] = new Entry("Old Kingdom, 3rd Dynasty (~2670 BCE)",
                "Painted limestone seated statue of Djoser, builder of the Step Pyramid at Saqqara. Found in the serdab — a sealed chamber with eye-slits — where the ka-statue could 'see' the offerings made for the king's eternal sustenance."),

            ["Nefertiti Bust"] = new Entry("New Kingdom, late 18th Dynasty, Amarna Period (~1345 BCE)",
                "The painted limestone bust of Queen Nefertiti, Great Royal Wife of Akhenaten. Discovered in the Amarna workshop of the sculptor Thutmose, the asymmetrical eyes and elegant elongated neck define the Amarna style."),

            ["Nofret"] = new Entry("Old Kingdom, 4th Dynasty (~2580 BCE)",
                "Painted limestone statue of Lady Nofret, wife of prince Rahotep. The inlaid rock-crystal eyes are so lifelike that the workmen who unearthed her at Meidum reportedly fled in fear. Among the most vivid portraits of Old Kingdom nobility."),

            ["Rahotep"] = new Entry("Old Kingdom, 4th Dynasty (~2580 BCE)",
                "Painted limestone statue of Prince Rahotep, son of Sneferu and overseer of works. Paired with his wife Nofret, the couple sit upright and frontal — the classic Old Kingdom convention asserting eternal presence."),

            ["Ramses III"] = new Entry("New Kingdom, 20th Dynasty (~1180 BCE)",
                "Image of Ramses III, last great warrior pharaoh, who repelled the Sea Peoples at the Battle of the Delta. His mortuary temple at Medinet Habu, with its famous battle reliefs, is one of the best-preserved royal complexes of Egypt."),

            ["Sandstone Sphinx Statue"] = new Entry("New Kingdom",
                "Sandstone sphinx with the body of a lion and the head of a king — a hybrid that fused royal authority with the protective power of a desert predator. Sphinxes lined processional avenues at Karnak and Luxor."),

            ["Sarcophagus of Hunefer"] = new Entry("New Kingdom, 19th Dynasty (~1290 BCE)",
                "Sarcophagus of Hunefer, royal scribe under Seti I. He is famous for his Book of the Dead papyrus, where the 'weighing of the heart' before Osiris is illustrated — his name has come to stand for the entire ritual."),

            ["Sarcophagus of Mindjedef"] = new Entry("Old Kingdom, 4th Dynasty (~2500 BCE)",
                "Stone sarcophagus of prince Mindjedef. The plain, palace-façade decoration is typical of Old Kingdom royal-relative burials at Giza, where the dead were laid in mastabas around the king's pyramid."),

            ["Sphinx of Hatshepsut"] = new Entry("New Kingdom, 18th Dynasty (~1470 BCE)",
                "Granite sphinx with the lioness body and the bearded male features of Hatshepsut, female pharaoh who ruled as king. After her death her successor Thutmose III ordered her images destroyed; this sphinx was reassembled from fragments at Deir el-Bahri."),

            ["Statue of Amenhotep III"] = new Entry("New Kingdom, 18th Dynasty (~1390–1352 BCE)",
                "Amenhotep III ruled a wealthy and peaceful Egypt at the height of New Kingdom power. His massive building program left Egypt with the temple of Luxor, the colossi of Memnon, and thousands of statues — including the Sekhmets seen in this museum."),

            ["Statue of Hatshepsut"] = new Entry("New Kingdom, 18th Dynasty (~1473–1458 BCE)",
                "Hatshepsut as kneeling king, presenting offering jars. One of the few female pharaohs, she expanded trade with Punt and built the terraced mortuary temple of Deir el-Bahri. She is shown here in male royal regalia, the kilt and false beard."),

            ["Statue of Maya and Merit"] = new Entry("New Kingdom, 18th Dynasty, post-Amarna (~1330 BCE)",
                "Maya, Tutankhamun's overseer of the treasury, with his wife Merit. Maya helped restore order after Akhenaten's revolution; his tomb at Saqqara was rediscovered in 1986 with these statues still intact in the chapel."),

            ["Statue of Mentuhotep II"] = new Entry("Middle Kingdom, 11th Dynasty (~2010 BCE)",
                "Mentuhotep II reunited Egypt after the chaos of the First Intermediate Period and founded the Middle Kingdom. His seated statue from his Deir el-Bahri mortuary complex shows him in the white crown of Upper Egypt and Osiride pose."),

            ["Statue of Ramses III"] = new Entry("New Kingdom, 20th Dynasty (~1180 BCE)",
                "Royal statue of Ramses III in the traditional smiting pose. He defended Egypt against the Sea Peoples and the Libyans; his temple at Medinet Habu preserves vivid reliefs of those battles, including the world's oldest detailed naval engagement."),

            ["Statue of Roy"] = new Entry("New Kingdom, 19th Dynasty (~1290 BCE)",
                "The royal scribe Roy, kneeling and presenting an offering. As royal scribe he managed temple revenues — literacy was a rare and powerful skill, and many high officials had themselves shown holding a scribal palette."),

            ["Statuette of a Jackal"] = new Entry("Late Period (~664–332 BCE)",
                "Recumbent jackal — Anubis, god of embalming and guardian of the necropolis. He oversaw the wrapping of the body and led the deceased to the Hall of Two Truths for the weighing of the heart."),

            ["Stela of Ituerneheh"] = new Entry("Middle Kingdom (~2055–1650 BCE)",
                "Funerary stela of Ituerneheh. The deceased is shown seated before a table piled with offerings; the carved formula invokes Osiris to grant 'a thousand of bread, beer, oxen and fowl' for the ka of the dead."),

            ["Stela of Seter-au"] = new Entry("Middle Kingdom to Second Intermediate Period",
                "Limestone funerary stela of Seter-au. Stelae like this stood at the entrance to the tomb chapel; passers-by who recited the offering prayer would feed the soul of the deceased — a kind of contract between the living and the dead."),

            ["Striding Thoth"] = new Entry("Late Period (~664–332 BCE)",
                "Striding statue of Thoth, ibis-headed god of writing, calculation, and the moon. Scribes invoked him at the start of every text. His main cult was at Hermopolis, where vast underground galleries hold mummified ibises offered to him."),

            ["The Goddess Mut"] = new Entry("New Kingdom, 18th Dynasty (~1390 BCE)",
                "Mut, the great mother goddess of Thebes and consort of Amun. Her temple at Karnak — with its sacred crescent lake — sits south of Amun's enclosure. She is most often shown wearing the double crown, sometimes with vulture headdress."),

            ["The Rosetta Stone"] = new Entry("Ptolemaic Period, reign of Ptolemy V (196 BCE)",
                "A decree of Ptolemy V issued in three scripts: hieroglyphic, demotic, and ancient Greek. Found by French soldiers at Rashid (Rosetta) in 1799, it gave Champollion the key to deciphering hieroglyphs in 1822 — the founding moment of Egyptology."),

            ["Triad of Menkaure"] = new Entry("Old Kingdom, 4th Dynasty (~2530 BCE)",
                "Greywacke triad: King Menkaure, builder of the third Giza pyramid, flanked by the goddess Hathor and a personification of one of Egypt's nomes. Several of these triads were carved for his pyramid temple — likely one per nome that supported the cult."),
        };

        [MenuItem(MenuPath)]
        public static void Apply() => DoApply(overwrite: false);

        [MenuItem(MenuPathOverwrite)]
        public static void ApplyOverwrite() => DoApply(overwrite: true);

        static void DoApply(bool overwrite)
        {
            var infos = Object.FindObjectsByType<ArtifactInfo>(FindObjectsInactive.Include);
            if (infos.Length == 0)
            {
                EditorUtility.DisplayDialog("Auto-Fill Artifact Info", "No ArtifactInfo components in active scene.", "OK");
                return;
            }

            int filled = 0, skipped = 0, missing = 0;
            var report = new System.Text.StringBuilder();

            foreach (var info in infos)
            {
                if (!Catalog.TryGetValue(info.gameObject.name, out var entry))
                {
                    missing++;
                    report.AppendLine($"  ? {info.gameObject.name} — no catalog entry");
                    continue;
                }

                bool changed = false;
                Undo.RecordObject(info, "Auto-Fill Artifact Info");

                if (string.IsNullOrEmpty(info.displayName) || overwrite) { info.displayName = info.gameObject.name; changed = true; }
                if (string.IsNullOrEmpty(info.era) || overwrite) { info.era = entry.era; changed = true; }
                if (string.IsNullOrEmpty(info.description) || overwrite) { info.description = entry.description; changed = true; }

                if (changed) { EditorUtility.SetDirty(info); filled++; report.AppendLine($"  + {info.gameObject.name}"); }
                else { skipped++; report.AppendLine($"  = {info.gameObject.name} (had values, kept)"); }
            }

            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log($"[Museum Artifact Auto-Fill]\n{report}");
            EditorUtility.DisplayDialog("Auto-Fill Artifact Info",
                $"Filled: {filled}\nSkipped (had values): {skipped}\nNo catalog entry: {missing}\nScene saved.\n\n" +
                (missing > 0 ? "Missing entries logged in Console — add them to the Catalog dictionary in this file." : "All artifacts in catalog."),
                "OK");
        }
    }
}
